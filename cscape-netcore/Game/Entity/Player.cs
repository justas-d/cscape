using System;
using System.Linq;
using CScape.Network;
using CScape.Network.Sync;
using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    public sealed class Player : AbstractEntity, IObserver
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

        public Logger Log => Server.Log;

        public MovementController Movement { get; }
        public Observatory Observatory { get; }

        /// <exception cref="ArgumentNullException"><paramref name="login"/> is <see langword="null"/></exception>
        public Player([NotNull] NormalPlayerLogin login) 
            : base(login.Server, 
                  login.Server.EntityIdPool,
                  new Transform(login.Data.X, login.Data.Y, login.Data.Z),
                  null) // todo : serialize personalized PoE's
        {
            if (login == null) throw new ArgumentNullException(nameof(login));

            Id = login.Data.Id;
            Username = login.Data.Username;
            PasswordHash = login.Data.PasswordHash;
            TitleIcon = login.Data.TitleIcon;

            Connection = new SocketContext(login.Server, login.Connection, login.SignlinkUid);
            Movement = new MovementController(Position);

            var obsSyncMachine = new ObservableSyncMachine(Server, this);
            Observatory = new Observatory(this, obsSyncMachine);

            Connection.SyncMachines.Add(new RegionSyncMachine(Server, Position));
            Connection.SyncMachines.Add(obsSyncMachine);

            Connection.SortSyncMachines();
        }

        public bool CanSee(AbstractEntity obs)
        {
            if (!PoE.ContainsObservable(obs))
                return false;

            if (obs is Player)
            {
                // todo : viewing distance that changes depending on the players we have to sync (so that we don't overflow)
            }

            const int range = 8; // todo : figure out how many tiles the player can see in total.

            return obs.Position.AbsoluteDistanceTo(Position) <= range;
        }

        public override void SyncObservable(ObservableSyncMachine sync, Blob blob, bool isNew)
        {
            if (isNew)
                sync.PushToPlayerSyncMachine(this);
        }
    }
}