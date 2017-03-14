using System;
using System.Net.Sockets;
using JetBrains.Annotations;

namespace cscape
{
    public sealed class Player : Entity
    {
        public class SocketContext
        {
            // todo : increase poll time if socket is experiencing disconnects?
            public int PollTime { get; } = 1000;

            /// <summary>
            /// In milliseconds, the delay between a socket dying and it's player being removed
            /// from the player entity pool. Default: 60 seconds.
            /// </summary>
            public long ReapTimeMs { get; } = 1000 * 60;
            private long _deadForMs = 0;

            [CanBeNull]
            public Socket Socket { get; private set; }

            public SocketContext([NotNull] Socket socket)
            {
                if (socket == null) throw new ArgumentNullException(nameof(socket));
                Socket = socket;
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
        public SocketContext Connection { get; set; }
        public int SignlinkId { get; }

        [NotNull]
        public GameServer Server { get; }

        public Player([NotNull] NormalPlayerLogin login)
        {
            if (login == null) throw new ArgumentNullException(nameof(login));
            Connection = new SocketContext(login.Connection);
            SignlinkId = login.SignlinkUid;

            Id = login.Data.Id;
            Username = login.Data.Username;
            PasswordHash = login.Data.PasswordHash;
            TitleIcon = login.Data.TitleIcon;
            Server = login.Server;
        }
    }
}