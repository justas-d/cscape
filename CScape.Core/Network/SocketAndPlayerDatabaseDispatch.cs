using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using CScape.Core.Game;
using CScape.Models;
using CScape.Models.Data;
using CScape.Models.Game;
using CScape.Models.Game.Entity.Factory;
using JetBrains.Annotations;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;

namespace CScape.Core.Network
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

    public static class Utils
    {
        public static OaepEncoding GetCrypto(IGameServerConfig cfg, bool forEncryption)
        {
            AsymmetricCipherKeyPair keys;
            using (var file = File.Open(cfg.PrivateLoginKeyDir, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var stream = new StreamReader(file))
                keys = (AsymmetricCipherKeyPair)new PemReader(stream).ReadObject();

            //todo: maybe switch to SHA256?
            var crypto = new OaepEncoding(new RsaEngine(), new Sha1Digest());
            if (forEncryption)
            {
                crypto.Init(true, keys.Public);
            }
            else
                crypto.Init(false, keys.Private);

            return crypto;
        }
    }

    /// <summary>
    /// Handles incoming connections and sets them up for the game loop.
    /// </summary>
    public class SocketAndPlayerDatabaseDispatch : IDisposable
    {
        [NotNull] private readonly IServiceProvider _services;
        [NotNull] private readonly IGameServer _server;
        [NotNull] private readonly IGameServerConfig _config;
        [NotNull] private readonly IPlayerDatabase _db;
        private readonly IPlayerFactory _players;
        [NotNull] private readonly Random _rng;
        [NotNull] private readonly Socket _socket;
        [NotNull] private readonly IAsymmetricBlockCipher _crypto;
        [NotNull] private readonly ILogger _log;
        [NotNull] private readonly Queue<IPlayerLogin> _loginQueue = new Queue<IPlayerLogin>();

        public bool IsDisposed { get; private set; }
        private bool _continueListening = true;
        public bool IsEnabled { get; set; } = true;

        public SocketAndPlayerDatabaseDispatch([NotNull] IServiceProvider services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));

            _players = services.ThrowOrGet<IPlayerFactory>();
            _db = services.ThrowOrGet<IPlayerDatabase>();
            _server = services.ThrowOrGet<IGameServer>();
            _config = services.ThrowOrGet<IGameServerConfig>();
            _log = services.ThrowOrGet<ILogger>();

            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
            {
                SendTimeout = _config.SocketSendTimeout,
                ReceiveTimeout = _config.SocketReceiveTimeout
            };
            _rng = new Random();

            _crypto = Utils.GetCrypto(_config, false);

            // start the service as soon as we're initialized
            Task.Run(StartListening).ContinueWith(t =>
            {
                _log.Debug(this, $"EntryPoint listen task terminated in status: Completed: {t.IsCompleted} Faulted: {t.IsFaulted} Cancelled: {t.IsCanceled}");
                if (t.Exception != null)
                    _log.Exception(this, "EntryPoint contained exception.", t.Exception);
            });
        }

        public IPlayerLogin TryGetNext()
            => _loginQueue.Count > 0 ? _loginQueue.Dequeue() : null;

        public async Task StartListening()
        {
            _socket.Bind(_config.ListenEndPoint);
            _socket.Listen(_config.Backlog);

            _log.Debug(this, "Entry point listening.");
            while (_continueListening)
            {
                Socket socket;
                try
                {
                    socket = await _socket.AcceptAsync();
                }
                catch (SocketException ex)
                {
                    _log.Exception(this, "Socket exception on main socket accept async.", ex);
                    continue;
                }
                catch (ObjectDisposedException ex)
                {
                    _log.Exception(this, "SocketDispatch threw disposed ex.", ex);
                    _continueListening = false;
                    IsDisposed = true;
                    break;
                }

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

                _log.Debug(this, "Initializing socket");

                var blob = new Blob(256);

                // initial handshake
                await SocketReceive(socket, blob, 2);

                var magic = blob.ReadByte();
                if (magic != initHandshakeMagic)
                {
                    _log.Debug(this, $"Killing socket due to back handshake magic ({magic})");
                    socket.Dispose();
                    return;
                }

                // another byte contains a bit of the username but we dont care about that
                blob.ReadCaret++;

                for (var i = 0; i < initMagicZeroCount; i++)
                    blob.Write(0);

                var sState = _server.GetState();
                if (sState.HasFlag(ServerStateFlags.PlayersFull))
                {
                    await KillBadConnection(socket, blob, InitResponseCode.WorldIsFull);
                    return;
                }
                if (sState.HasFlag(ServerStateFlags.LoginDisabled))
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

                // todo : catch crypto exceptions
                var decrypted = _crypto.ProcessBlock(blob.Buffer, 0, blob.Buffer.Length);
                blob.Overwrite(decrypted, 0, 0);

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
                if (revision != _config.Revision)
                {
                    await KillBadConnection(socket, blob, InitResponseCode.MustUpdate);
                    return;
                }

                var isLowMem = blob.ReadByte() == 1;

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
                if (!blob.TryReadString(out username, PlayerModel.MaxUsernameChars))
                {
                    await KillBadConnection(socket, blob, InitResponseCode.GeneralFailure,
                        "Overflow detected when reading username.");
                    return;
                }

                if (!blob.TryReadString(out password, PlayerModel.MaxPasswordChars))
                {
                    await KillBadConnection(socket, blob, InitResponseCode.GeneralFailure,
                        "Overflow detected when reading password.");
                    return;
                }

                username = username.ToLowerInvariant();

                // check if user is logged in
                var loggedInPlayer = _players.Get(username);

                if (isReconnecting)
                {
                    // check if valid user
                    if (loggedInPlayer == null)
                    {
                        await KillBadConnection(socket, blob, InitResponseCode.GeneralFailure,
                            "Tried to reconnect to player that is not present in ent pool.");
                        return;
                    }

                    // check if we can reconnect
                    if (!loggedInPlayer.Connection.CanReinitialize(signlinkUid))

                    {
                        await KillBadConnection(socket, blob, InitResponseCode.GeneralFailure,
                            "Tried to reconnect but player is not available for reconnecting.");
                        return;
                    }

                    // check if the password matches
                    if (!await _db.IsValidPassword(loggedInPlayer.Password, password))
                    {
                        await KillBadConnection(socket, blob, InitResponseCode.InvalidCredentials);
                        return;
                    }

                    // all's good, queue reconnect.
                    blob.Write((byte) InitResponseCode.ReconnectDone);
                    _loginQueue.Enqueue(new ReconnectPlayerLogin(loggedInPlayer, socket, signlinkUid));
                }
                else
                {
                    if (loggedInPlayer != null)
                    {
                        await KillBadConnection(socket, blob, InitResponseCode.AccountAlreadyLoggedIn);
                        return;
                    }

                    IPlayerModel model;

                    // todo : less calls to _db.GetPlayer() in basic ILoginService
                    if (await _db.GetPlayer(username) == null)
                    {
                        model = await _db.CreatePlayer(username, password);
                    }
                    else
                    {
                        model = await _db.GetPlayer(username, password);
                    }

                    if (model == null)
                    {
                        await KillBadConnection(socket, blob, InitResponseCode.InvalidCredentials);
                        return;
                    }

                    blob.Write((byte) InitResponseCode.LoginDone);
                    blob.Write(0); // is flagged
                    blob.Write(model.TitleIcon);
                    _loginQueue.Enqueue(new NormalPlayerLogin(_services, model, socket, signlinkUid, !isLowMem));
                }

                await SocketSend(socket, blob);

                socket.Blocking = false;
                _log.Debug(this, "Done socket init.");
            }
            catch (SocketException)
            {
                _log.Debug(this, "SocketException in Entry.");
            }
            catch (ObjectDisposedException)
            {
                _log.Debug(this, "ObjectDisposedException in Entry.");
            }
            catch (CryptoException cryptEx)
            {
                _log.Exception(this, "Crypto Exception in EntryPoint.", cryptEx);
            }
            catch (Exception ex)
            {
                _log.Exception(this, "Unhandled exception in EntryPoint.", ex);
                Debug.Fail(ex.ToString());
            }
        }

        private async Task KillBadConnection(Socket socket, Blob blob, InitResponseCode response, string log = null)
        {
            blob.Write((byte)response);
            await SocketSend(socket, blob);
            socket?.Dispose();
            if (log != null)
                _log.Warning(this, log);
        }

        private static Task<int> SocketSend(Socket socket, Blob blob)
        {
            var task = socket.SendAsync(new ArraySegment<byte>(blob.Buffer, 0, blob.WriteCaret), SocketFlags.None);
            blob.ResetHeads();
            return task;
        }

        private static Task<int> SocketReceive(Socket socket, Blob blob, int len)
        {
            var task = socket.ReceiveAsync(new ArraySegment<byte>(blob.Buffer, 0, len), SocketFlags.None);
            blob.ResetHeads();
            return task;
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                _socket?.Dispose();
                _continueListening = false;
            }
        }
    }
}