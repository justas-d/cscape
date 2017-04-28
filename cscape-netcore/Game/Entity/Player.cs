using System;
using System.Diagnostics;
using CScape.Data;
using CScape.Network;
using CScape.Network.Sync;
using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    //todo: change username feature
    //todo: change password feature

    public interface IMovingEntity : IEntity
    {
        Transform Position { get; }
        MovementController Movement { get; }
    }

    [DebuggerDisplay("Name {Username}")]
    public sealed class Player : AbstractEntity, IObserver, IMovingEntity
    {
        public int Pid { get; }

        #region sync vars

        [Flags]
        public enum UpdateFlags
        {
            Chat = 0x80,
            InteractEnt = 0x1,
            Appearance = 0x10,
            FacingCoordinate = 0x2,
        }

        public UpdateFlags Flags { get; private set; }

        public void SetFlag(UpdateFlags flag)
            => Flags |= flag;


        [CanBeNull] private ChatMessage _lastChatMessage;

        [CanBeNull] public ChatMessage LastChatMessage
        {
            get => _lastChatMessage;
            set
            {
                _lastChatMessage = value;
                SetFlag(UpdateFlags.Chat);
            }
        }

        [NotNull] private PlayerAppearance _appearance;
        [NotNull]
        public PlayerAppearance Appearance
        {
            get => _appearance;
            set
            {
                // ReSharper disable once ConstantNullCoalescingCondition
                var val = value ?? PlayerAppearance.Default;
                _appearance = val;
                _model.SetAppearance(val);
                SetFlag(UpdateFlags.Appearance);
            }
        }
        

        #endregion

        [NotNull] public string Username => _model.Username;
        [NotNull] public string Password => _model.PasswordHash;
        public byte TitleIcon => _model.TitleIcon;

        [NotNull] public SocketContext Connection { get; }
        [NotNull] public Logger Log => Server.Log;
        [NotNull] public Observatory Observatory { get; }
        [NotNull] private readonly PlayerModel _model;
        [NotNull] public MovementController Movement { get; }

        public bool NeedsInitWhenLocal { get; private set; } = true;
        public bool TeleporToDestWhenWalking { get; private set; }

        /// <exception cref="ArgumentNullException"><paramref name="login"/> is <see langword="null"/></exception>
        public Player([NotNull] NormalPlayerLogin login) 
            : base(login.Server, 
                  login.Server.EntityIdPool,
                  login.Model.X, login.Model.Y, login.Model.Z)
        {
            if (login == null) throw new ArgumentNullException(nameof(login));

            _model = login.Model;
            Appearance = new PlayerAppearance(_model);
            Pid = Convert.ToInt32(login.Server.PlayerIdPool.NextId());
            Movement = new MovementController(this);

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
            // sync db model
            _model.SetPosition(Position);

            // reset sync vars
            Flags = 0;
            NeedsInitWhenLocal = false;


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
                    Server.SavePlayers();

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

        /// <summary>
        /// Forcibly teleports the player to the given coords.
        /// Use this instead of manually setting player position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void ForceTeleport(ushort x, ushort y, byte z)
        {
            if (Position.X == x && Position.Y == y && Position.Z == z)
                return;

            Position.SetPosition(x,y,z);
            NeedsInitWhenLocal = true;
        }

        public void ForceTeleport(ushort x, ushort y)
            => ForceTeleport(x, y, Position.Z);

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