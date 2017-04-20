using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using JetBrains.Annotations;

namespace cscape
{
    public sealed class Player : Entity
    {
        public class SocketContext
        {
            public const int OutStreamSize = byte.MaxValue + 1 * 128; //~62.5MB total mem used with 2000 concurrent players.

            // todo : increase poll time if socket is experiencing disconnects?
            public int PollTime { get; } = 1000;

            /// <summary>
            /// In milliseconds, the delay between a socket dying and it's player being removed
            /// from the player entity pool. todo Default: 60 seconds.
            /// </summary>
            public long ReapTimeMs { get; } = 1000 * 2;
            private long _deadForMs = 0;

            [CanBeNull]
            public Socket Socket { get; private set; }

            [NotNull]
            public Blob OutStream { get; }

            public int SignlinkId { get; }

            [NotNull]
            public readonly List<SyncMachine> SyncMachines = new List<SyncMachine>();

            /// <exception cref="ArgumentNullException"><paramref name="socket"/> is <see langword="null"/></exception>
            public SocketContext([NotNull] Socket socket, int signLink)
            {
                Socket = socket ?? throw new ArgumentNullException(nameof(socket));
                Socket.Blocking = false;

                OutStream = new Blob(OutStreamSize);
                SignlinkId = signLink;
            }

            /// <exception cref="ArgumentNullException"><paramref name="socket"/> is <see langword="null"/></exception>
            public void AssignNewSocket([NotNull] Socket socket)
            {
                Socket = socket ?? throw new ArgumentNullException(nameof(socket));
                _deadForMs = 0;
            }

            public bool IsConnected()
            {
                if (Socket == null) return false;

                var part1 = Socket.Poll(PollTime, SelectMode.SelectRead);
                var part2 = Socket.Available == 0;
                if (part1 && part2)
                    return false;
                return true;
            }

            public void SendOutStream()
            {
                // return if we're haven't actually written anything to the output blob
                if (OutStream.BytesWritten <= 0) return;

                try
                {
                    Socket?.Send(OutStream.Buffer, 0, OutStream.BytesWritten, SocketFlags.None);
                }
                catch (SocketException)
                {
                    Socket?.Dispose();
                    Socket = null;
                }
                catch (ObjectDisposedException)
                {
                    Socket = null;
                }
                finally
                {
                    OutStream.ResetWrite();
                }
            }

            /// <summary>
            /// Checks for "hard disconnects" (force closes of the client/socket).
            /// Manages the state of the socket while in one. 
            /// </summary>
            /// <param name="time">Milliseconds that have elapsed since the last call to this method.</param>
            /// <returns>Returns true when the owning player can be removed from the</returns>
            public bool ManageHardDisconnect(long time)
            {
                if (IsConnected()) return false;

                Socket = null;
                _deadForMs = _deadForMs + time;
                return _deadForMs >= ReapTimeMs;
            }
        }

        public const int MaxUsernameChars = 12;
        public const int MaxPasswordChars = 128;

        public int Id { get; }
        //todo: change username feature

        [NotNull]
        public string Username { get; private set; }
        //todo: change password feature
        [NotNull]
        public string PasswordHash { get; private set; }

        public byte TitleIcon { get; set; }

        [NotNull]
        public SocketContext Connection { get; }

        [NotNull]
        public GameServer Server { get; }

        public override PositionController Position { get; }

        /// <exception cref="ArgumentNullException"><paramref name="login"/> is <see langword="null"/></exception>
        public Player([NotNull] NormalPlayerLogin login)
        {
            if (login == null) throw new ArgumentNullException(nameof(login));
            Connection = new SocketContext(login.Connection, login.SignlinkUid);

            Id = login.Data.Id;
            Username = login.Data.Username;
            PasswordHash = login.Data.PasswordHash;
            TitleIcon = login.Data.TitleIcon;
            Server = login.Server;
            Position = new PositionController(login.Data.X, login.Data.Y, login.Data.Z);
        

            Connection.SyncMachines.Add(new RegionSyncMachine(Server, Position));
            Connection.SyncMachines.Add(new PlayerUpdateSyncMachine(Server, this));
        }

    }
}