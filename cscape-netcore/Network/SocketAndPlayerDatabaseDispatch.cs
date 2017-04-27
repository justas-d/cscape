using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using CScape.Data;
using CScape.Game.Entity;
using JetBrains.Annotations;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;

namespace CScape.Network
{
    public enum InitResponseCode : byte
    {
        ContinueToCredentials = 0,
        LoginDone = 2,
        ReconnectDone = 15,

        Wait = 1,
        InvalidCredentials = 3,
        DisabledAccount = 4,
        AccountAlreadyLoggedIn = 5,
        MustUpdate = 6,
        WorldIsFull = 7,
        LoginServerOffline = 8,
        LoginRatelimitByAddress = 9,
        BadSessionId = 10, 
        LoginServerRejected = 11,
        IsNotAMember = 12,
        GeneralFailure = 13,
        UpdateInProgress = 14,
        LoginRatelimitBySocket = 16,
        InMembersArea = 17,
        InvalidLoginServer = 20,
        TransferringAccount = 21, // send extra byte for countdown on the client's end
        // todo : ratelimiting for login requests
    }

    /// <summary>
    /// Handles incoming connections and sets them up for the game loop.
    /// </summary>
    public class SocketAndPlayerDatabaseDispatch : IDisposable
    {
        private readonly ConcurrentQueue<IPlayerLogin> _loginQueue;
        public GameServer Server { get; }
        public EndPoint Endpoint { get; }
        public int Backlog { get; }

        private readonly Random _rng;
        private readonly Socket _socket;

        private readonly IAsymmetricBlockCipher _crypto;

        private readonly object _saveLock = new object();
        private bool _saveFlag;

        public bool SaveFlag
        {
            get
            {
                lock (_saveLock) return _saveFlag;
            }
            set
            {
                lock (_saveLock) _saveFlag = value;
            }
        }


        public SocketAndPlayerDatabaseDispatch(GameServer server, [NotNull] ConcurrentQueue<IPlayerLogin> loginQueue)
        {
            _loginQueue = loginQueue ?? throw new ArgumentNullException(nameof(loginQueue));
            Server = server;
            Endpoint = server.Config.ListenEndPoint;
            Backlog = Convert.ToInt32(server.Config.Backlog);
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _socket.SendTimeout = 5000;
            _socket.ReceiveTimeout = 5000;
            _rng = new Random();

            AsymmetricCipherKeyPair keys;
            using (var file = File.Open(Server.Config.PrivateLoginKeyDir, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var stream = new StreamReader(file))
                keys = (AsymmetricCipherKeyPair)new PemReader(stream).ReadObject();

            //todo: maybe switch to SHA256?
            _crypto = new OaepEncoding(new RsaEngine(), new Sha1Digest());
            _crypto.Init(false, keys.Private);
        }

        public async Task StartListening()
        {
            _socket.Bind(Endpoint);
            _socket.Listen(Backlog);

            Server.Log.Debug(this, "Entry point listening.");

            while (true)
            {
                if (SaveFlag)
                {
                    await Server.Database.Player.Save();
                    Server.Log.Debug(this, $"Saving db");
                    SaveFlag = false;
                }

                var socket = await _socket.AcceptAsync();
                if (socket == null || !socket.Connected)
                    continue;

                await InitConnection(socket);
            }
        }

        private async Task InitConnection(Socket socket)
        {
            try
            {
                const int loginMagic = 10;
                const int reconnectMagic = 18;
                const int normalConnectMagic = 16;
                const int crcCount = 9;
                const int initMagicZeroCount = 8;
                const int initHandshakeMagic = 14;
                const int keyCount = 4;
                const int randomKeySize = sizeof(long);

                Server.Log.Debug(this, "Initializing socket");

                var blob = new Blob(256);

                // initial handshake
                await SocketReceive(socket, blob, 2);

                var magic = blob.ReadByte();
                if (magic != initHandshakeMagic)
                {
                    Server.Log.Debug(this, $"Killing socket due to back handshake magic ({magic})");
                    socket.Dispose();
                    return;
                }

                // another byte contains a bit of the username but we dont care about that
                blob.ReadCaret++;

                for (var i = 0; i < initMagicZeroCount; i++)
                    blob.Write(0);

                var sState = Server.GetState();
                if (sState.HasFlag(GameServer.ServerStateFlags.PlayersFull))
                {
                    await KillBadConnection(socket, blob, InitResponseCode.WorldIsFull);
                    return;
                }
                if (sState.HasFlag(GameServer.ServerStateFlags.LoginDisabled))
                {
                    await KillBadConnection(socket, blob, InitResponseCode.LoginServerOffline);
                    return;
                }

                // initMagicZeroCount can be any InitResponseCode
                blob.Write((byte) InitResponseCode.ContinueToCredentials);

                // write server isaac key
                var serverKey = new byte[randomKeySize];
                _rng.NextBytes(serverKey);
                blob.WriteBlock(serverKey, 0, randomKeySize);

                // send the packet
                await SocketSend(socket, blob);

                // receive login block
                // header
                await SocketReceive(socket, blob, blob.Buffer.Length);
                blob.Overwrite(_crypto.ProcessBlock(blob.Buffer, 0, blob.Buffer.Length));

                // verify login header magic
                magic = blob.ReadByte();
                if (magic != normalConnectMagic && magic != reconnectMagic)
                {
                    await KillBadConnection(socket, blob, InitResponseCode.GeneralFailure,
                        $"Invalid login block magic: {magic}");
                    return;
                }

                var isReconnecting = magic == reconnectMagic;

                //1 - length
                //2  - 255
                // skip 'em
                blob.ReadCaret += 2;

                // verify revision
                var revision = blob.ReadInt16();
                if (revision != Server.Config.Revision)
                {
                    await KillBadConnection(socket, blob, InitResponseCode.MustUpdate);
                    return;
                }

                blob.ReadByte(); // low mem
                //var isLowMem = blob.ReadByte() == 1;

                // read crcs
                var crcs = new int[crcCount];
                for (var i = 0; i < crcCount; i++)
                    crcs[i] = blob.ReadInt32();

                // login block
                // check login magic
                magic = blob.ReadByte();
                if (magic != loginMagic)
                {
                    await KillBadConnection(socket, blob, InitResponseCode.GeneralFailure,
                        $"Invalid login magic: {magic}");
                    return;
                }

                // read client&server keys
                var keys = new int[keyCount];
                for (var i = 0; i < keyCount; i++)
                    keys[i] = blob.ReadInt32();

                var signlinkUid = blob.ReadInt32();

                // try read user/pass
                string username;
                string password;
                if (!blob.TryReadString(PlayerModel.MaxUsernameChars, out username))
                {
                    await KillBadConnection(socket, blob, InitResponseCode.GeneralFailure,
                        "Overflow detected when reading username.");
                    return;
                }

                if (!blob.TryReadString(PlayerModel.MaxPasswordChars, out password))
                {
                    await KillBadConnection(socket, blob, InitResponseCode.GeneralFailure,
                        "Overflow detected when reading password.");
                    return;
                }
                    
                username = username.ToLowerInvariant();

                // check if user is logged in
                var loggedInPlayer = Server.Players.FirstOrDefault(
                    p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

                if (isReconnecting)
                {
                    if (loggedInPlayer == null)
                    {
                        await KillBadConnection(socket, blob, InitResponseCode.GeneralFailure,
                            "Tried to reconnect to player that is not present in ent pool.");
                        return;
                    }

                    if (!await Server.Database.Player.IsValidPassword(loggedInPlayer.Password, password))
                    {
                        await KillBadConnection(socket, blob, InitResponseCode.InvalidCredentials);
                        return;
                    }

                    blob.Write((byte)InitResponseCode.ReconnectDone);
                    _loginQueue.Enqueue(new ReconnectPlayerLogin(loggedInPlayer, socket, signlinkUid));
                }
                else
                {
                    if (loggedInPlayer != null)
                    {
                        await KillBadConnection(socket, blob, InitResponseCode.AccountAlreadyLoggedIn);
                        return;
                    }

                    PlayerModel model;
                    if (await Server.Database.Player.GetPlayer(username) == null)
                    {
                        model = await Server.Database.Player.CreatePlayer(username, password);
                    }
                    else
                    {
                        model = await Server.Database.Player.GetPlayer(username, password);
                    }

                    if (model == null)
                    {
                        await KillBadConnection(socket, blob, InitResponseCode.InvalidCredentials);
                        return;
                    }

                    blob.Write((byte)InitResponseCode.LoginDone);
                    blob.Write(0); // is flagged
                    blob.Write(model.TitleIcon);
                    _loginQueue.Enqueue(new NormalPlayerLogin(Server, model, socket, signlinkUid));
                }

                await SocketSend(socket, blob);

                socket.Blocking = false;
                Server.Log.Debug(this, "Done socket init.");
            }
            catch (SocketException)
            {
                Server.Log.Debug(this, "SocketException in Entry.");
            }
            catch (ObjectDisposedException)
            {
                Server.Log.Debug(this, "ObjectDisposedException in Entry.");
            }
            catch (Exception ex)
            {
                Server.Log.Exception(this, "Unhandled exception in EntryPoint.", ex);
                Debug.Fail(ex.ToString());
            }
        }

        private async Task KillBadConnection(Socket socket, Blob blob, InitResponseCode response, string log = null)
        {
            blob.Write((byte)response);
            await SocketSend(socket, blob);
            socket?.Dispose();
            if (log != null)
                Server.Log.Warning(this, log);
        }

        private static Task<int> SocketSend(Socket socket, Blob blob)
        {
            var task = socket.SendAsync(new ArraySegment<byte>(blob.Buffer, 0, blob.WriteCaret), SocketFlags.None);
            blob.ResetWrite();
            return task;
        }

        private static Task<int> SocketReceive(Socket socket, Blob blob, int len)
            => socket.ReceiveAsync(new ArraySegment<byte>(blob.Buffer, 0, len), SocketFlags.None);

        public void Dispose()
        {
            _socket?.Dispose();
        }
    }
}