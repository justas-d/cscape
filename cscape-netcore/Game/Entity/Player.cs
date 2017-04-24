using System;
using System.Diagnostics;
using CScape.Network;
using CScape.Network.Packet;
using CScape.Network.Sync;
using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    public sealed class Player : AbstractEntity, IObserver, IPlayerSaveData
    {
        public const int MaxUsernameChars = 12;
        public const int MaxPasswordChars = 128;

        public int DatabaseId { get; }
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
                  login.Data.X, login.Data.Y, login.Data.Z)
        {
            if (login == null) throw new ArgumentNullException(nameof(login));

            DatabaseId = login.Data.DatabaseId;
            Username = login.Data.Username;
            PasswordHash = login.Data.PasswordHash;
            TitleIcon = login.Data.TitleIcon;

            Connection = new SocketContext(this, login.Server, login.Connection, login.SignlinkUid);

            var obsSyncMachine = new ObservableSyncMachine(Server, this);
            Observatory = new Observatory(this, obsSyncMachine);

            Connection.SyncMachines.Add(new RegionSyncMachine(Server, Position));
            Connection.SyncMachines.Add(obsSyncMachine);

            Connection.SortSyncMachines();

            // todo : serialize personalized PoE's
            InitPoE(null, Server.Overworld);
            Server.RegisterNewPlayer(this);
        }

        public override void Update(MainLoop loop)
        {
            // todo : figure out if we need to update a player/abstract entity if they have been Destroy()'ed.

            if (IsDestroyed)
            {
                var msg = $"Updating destroyed player {Username}";
                Log.Warning(this, msg);
                Debug.Fail(msg);
            }

            // check for hard disconnects
            // returning true would mean that we need to reap the player out of the world.
            // false indicates that the connection is still good, or the connection has been reaped and we need to keep the player alive until the method says otherwise.
            if (Connection.ManageHardDisconnect(loop.DeltaTime + loop.ElapsedMilliseconds))
            {
                Log.Debug(this, $"Reaping {Username}");
                Destroy();
                return;
            }

            if (Connection.IsConnected())
            {
                // if the logoff flag is set, log the player off.
                if (LogoutMethod != LogoutType.None)
                {
                    Connection.Dispose(); // shut down the connection
                    Save(); // queue save

                    // queue the player for removal from playing list, since they cleanly logged out.
                    if (LogoutMethod == LogoutType.Clean)
                    {
                        Destroy();
                        return;
                    }
                        
                }
            }

            loop.Player.Enqueue(this);
        }

        protected override void InternalDestroy()
            => Server.UnregisterPlayer(this);

        public bool CanSee(AbstractEntity obs)
        {
            if (!PoE.ContainsEntity(obs))
                return false;

            const int maxrange = 15;

            if (obs is Player)
            {
                // todo : adjust maxrange if the player update packet gets too big or too small.
                // keep the max at 15, min at 0.
                
            }

            return Math.Abs(obs.Position.MaxDistanceTo(Position)) <= maxrange;
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
            LogoffPacket.Static.Send(Connection.OutStream);
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
            LogoffPacket.Static.Send(Connection.OutStream);
        }

        private LogoutType LogoutMethod { get; set; }

        private enum LogoutType
        {
            None,
            Clean,
            Forced
        }
    }
}