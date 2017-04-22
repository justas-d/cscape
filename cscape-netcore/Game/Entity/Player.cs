using System;
using CScape.Network;
using CScape.Network.Sync;
using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    public sealed class Player : AbstractEntity, IObserver, IPlayerSaveData
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
        public ushort X => Position.X;
        public ushort Y => Position.Y;
        public byte Z => Position.Z;

        [NotNull]
        public SocketContext Connection { get; }

        public Logger Log => Server.Log;

        public Observatory Observatory { get; }

        /// <exception cref="ArgumentNullException"><paramref name="login"/> is <see langword="null"/></exception>
        public Player([NotNull] NormalPlayerLogin login) 
            : base(login.Server, 
                  login.Server.EntityIdPool,
                  login.Data.X, login.Data.Y, login.Data.Z,
                  null,// todo : serialize personalized PoE's
                  true) 
        {
            if (login == null) throw new ArgumentNullException(nameof(login));

            Id = login.Data.Id;
            Username = login.Data.Username;
            PasswordHash = login.Data.PasswordHash;
            TitleIcon = login.Data.TitleIcon;

            Connection = new SocketContext(this, login.Server, login.Connection, login.SignlinkUid);

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

        /// <summary>
        /// Queues a player for saving.
        /// </summary>
        public void Save()
            => Server.InternalEntry.SaveQueue.Enqueue(this);

        /// <summary>
        /// Sends a system chat message to this player.
        /// </summary>
        public void SendSystemChatMessage(string msg)
            => Connection.SendMessage(new SystemChatMessagePacket(msg));

        /// <summary>
        /// Provides a way to cleanly logout of the world.
        /// Imposes checks to make sure the player doesn't logout when they can't.
        /// Socket is immediatelly closed.
        /// Player data is saved.
        /// </summary>
        /// <param name="reason">The reason for which the player cannot logout.</param>
        /// <returns>Can or cannot the player logout.</returns>
        public bool Logout([CanBeNull] out string reason)
        {
            reason = null;

            if (LogoutMethod != LogoutType.None)
                return false;

            // todo : do logoff checks, i.e in combat or something

            LogoutMethod = LogoutType.Clean;
            LogoutManager.PreLogout(this);
            return true;
        }

        /// <summary>
        /// Sends a logoff packet then forcefully drops the connection. 
        /// Keeps the player alive in the world.
        /// Should only be used when something goes wrong.
        /// </summary>
        public void ForcedLogout()
        {
            if (LogoutMethod != LogoutType.None)
                return;

            LogoutMethod = LogoutType.Forced;
            LogoutManager.PreLogout(this);
        }

        internal LogoutType LogoutMethod { get; set; }

        internal enum LogoutType
        {
            None,
            Clean,
            Forced
        }
    }
}