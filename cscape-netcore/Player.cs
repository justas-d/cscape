using System;
using JetBrains.Annotations;

namespace cscape
{
    public sealed partial class Player : Entity
    {
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

        public Logger Log => Server.Log;

        public override PositionController Position { get; }

        /// <exception cref="ArgumentNullException"><paramref name="login"/> is <see langword="null"/></exception>
        public Player([NotNull] NormalPlayerLogin login)
        {
            if (login == null) throw new ArgumentNullException(nameof(login));

            Id = login.Data.Id;
            Username = login.Data.Username;
            PasswordHash = login.Data.PasswordHash;
            TitleIcon = login.Data.TitleIcon;
            Server = login.Server;
            Position = new PositionController(login.Data.X, login.Data.Y, login.Data.Z);
            Connection = new SocketContext(login.Server, login.Connection, login.SignlinkUid);

            Connection.SyncMachines.Add(new RegionSyncMachine(Server, Position));
            Connection.SyncMachines.Add(new PlayerUpdateSyncMachine(Server, this));
        }
    }
}