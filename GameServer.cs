using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace cscape
{
    public class GameServer
    {
    }

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

        public PlayerEntryPoint(GameServer server, EndPoint endpoint, int backlog)
        {
            Server = server;
            Endpoint = endpoint;
            Backlog = backlog;
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _rng = new Random();
        }

        public async Task StartListening()
        {
            _socket.Bind(Endpoint);
            _socket.Listen(Backlog);

            while (true)
            {
                //@TODO: exception handle socket acception
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
                const int initBufSize = 512;
                const int initHandshakeMagic = 14;

                var buf = new byte[initBufSize];

                // initial handshake
                await SocketReceive(socket, buf, 0, 2);
                if (buf[0] != initHandshakeMagic)
                {
                    KillSocket(socket);
                    return;
                }

                // another byte contains a bit of the username but we dont care about that

                const int initMagicZeroCount = 8;
                for (var i = 0; i < initMagicZeroCount; i++)
                    buf[i] = 0;

                // initMagicZeroCount can be any InitResponseCode
                buf[initMagicZeroCount] = (byte) InitResponseCode.ContinueToCredentials;

                await SocketSend(socket, buf, 0, initMagicZeroCount + 1);

                const int randomKeySize = sizeof(long);
                var serverKey = new byte[randomKeySize];
                _rng.NextBytes(serverKey);

            }
            catch
            {

            }
        }

        private static void KillSocket(Socket socket)
            => socket?.Dispose();

        private async Task<int> SocketSend(Socket socket, byte[] buffer, int off, int len)
            => await Task.Factory.FromAsync((c, o) => socket.BeginSend(buffer, off, len, SocketFlags.None, c, o),
                socket.EndSend, null);

        private static async Task<int> SocketReceive(Socket socket, byte[] buffer, int off, int len)
            => await Task.Factory.FromAsync((c, o) => socket.BeginReceive(buffer, off, len, SocketFlags.None, c, o),
                socket.EndReceive, null);

        public void Dispose()
        {
            _rng?.Dispose();
            _socket?.Dispose();
        }
    }
}
