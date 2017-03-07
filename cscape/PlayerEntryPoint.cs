using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;

namespace cscape
{
    /// <summary>
    /// Handles incoming connections and sets them up for the game loop.
    /// </summary>
    public class PlayerEntryPoint : IDisposable
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
            LoginRatelimitBySocket= 16,
            InMembersArea = 17,
            InvalidLoginServer = 20,
            TransferringAccount = 21, // send extra byte for countdown on the client's end
        }

        public GameServer Server { get; }
        public EndPoint Endpoint { get; }
        public int Backlog { get; }

        private readonly Random _rng;
        private readonly Socket _socket;

        private readonly IAsymmetricBlockCipher _crypto;

        public PlayerEntryPoint(GameServer server)
        {
            Server = server;
            Endpoint = server.Config.ListenEndPoint;
            Backlog = server.Config.Backlog;
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _rng = new Random();

            AsymmetricCipherKeyPair keys;
            using (var file = File.Open(Server.Config.PrivateLoginKeyDir, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var stream = new StreamReader(file))
                keys = (AsymmetricCipherKeyPair) new PemReader(stream).ReadObject();

            //@TODO: maybe switch to SHA256?
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
                var socket = await Task.Factory.FromAsync(_socket.BeginAccept, _socket.EndAccept, null);
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
                    KillSocket(socket);
                    return;
                }

                // another byte contains a bit of the username but we dont care about that
                blob.ReadHead++;

                for (var i = 0; i < initMagicZeroCount; i++)
                    blob.Write(0);

                // initMagicZeroCount can be any InitResponseCode
                // @TODO some sort of function that inspects the state of the server and returns an appropriate InitResponseCode
                blob.Write((byte)InitResponseCode.ContinueToCredentials);

                // write server isaac key
                var serverKey = new byte[randomKeySize];
                _rng.NextBytes(serverKey);
                blob.WriteBlock(serverKey, 0, randomKeySize);

                // send the packet
                await SocketSend(socket, blob, blob.WriteHead + 1);
                blob.ResetWrite();

                // receive login block
                // header
                await SocketReceive(socket, blob, blob.Buffer.Length);
                blob.Overwrite(_crypto.ProcessBlock(blob.Buffer, 0, blob.Buffer.Length));

                // verify login header magic
                magic = blob.ReadByte();
                if (magic != normalConnectMagic && magic != reconnectMagic)
                {
                    Server.Log.Warning(this, $"Invalid login block magic: {magic}");
                    KillSocket(socket);
                    return;
                }

                //@TODO reconnect logic
                var isReconnecting = magic == reconnectMagic;

                //1 - length
                //2  - 255
                // skip 'em
                blob.ReadHead += 2;

                // verify revision
                var revision = blob.ReadInt16();
                if (revision != Server.Config.Revision)
                {
                    Server.Log.Warning(this, $"Invalid revision: {revision}");
                    KillSocket(socket);
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
                    Server.Log.Warning(this, $"Invalid login magic: {magic}");
                    KillSocket(socket);
                    return;
                }

                // read client&server keys
                var keys = new int[keyCount];
                for (var i = 0; i < keyCount; i++)
                    keys[i] = blob.ReadInt32();

                //@TODO reconnect logic
                var signlinkUid = blob.ReadInt32();

                // try read user/pass
                string username;
                string password;
                if (!blob.TryReadString(Player.MaxUsernameChars, out username))
                {
                    Server.Log.Warning(this, $"Overflow detected when reading username.");
                    KillSocket(socket);
                    return;
                }

                if (!blob.TryReadString(Player.MaxPasswordChars, out password))
                {
                    Server.Log.Warning(this, $"Overflow detected when reading password.");
                    KillSocket(socket);
                    return;
                }

                Server.Log.Debug(this, "Done socket init.");
            }
            //@TODO: exception handle socket acception
            catch
            {
                throw;
            }
        }

        private static void KillSocket(Socket socket)
            => socket?.Dispose();

        private async Task<int> SocketSend(Socket socket, Blob blob, int len)
            => await Task.Factory.FromAsync((c, o) => socket.BeginSend(blob.Buffer, 0, len, SocketFlags.None, c, o),
                socket.EndSend, null);

        private static async Task<int> SocketReceive(Socket socket, Blob blob, int len)
            => await Task.Factory.FromAsync((c, o) => socket.BeginReceive(blob.Buffer, 0, len, SocketFlags.None, c, o),
                socket.EndReceive, null);

        public void Dispose()
        {
            _socket?.Dispose();
        }
    }
}