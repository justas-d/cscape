using System;
using System.Diagnostics;
using CScape.Data;
using CScape.Game.World;
using CScape.Network;
using CScape.Network.Sync;
using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    //todo: change username feature
    //todo: change password feature

    public sealed class Player : WorldEntity, IMovingEntity, IObserver
    {
        #region debug vars

        public bool DebugCommands = false;
        public bool DebugPackets = false;
        public bool DebugRegion = true;
        public bool DebugStats
        {
            get => _debugStatSync?.IsEnabled ?? false;
            set
            {
                if (value && _debugStatSync == null)
                {
                    _debugStatSync = new DebugStatSyncMachine(Server);
                    Connection.SyncMachines.Add(_debugStatSync);
                    Connection.SortSyncMachines();
                }

                _debugStatSync.IsEnabled = value;
            }
        }

        private DebugStatSyncMachine _debugStatSync;

        #endregion

        #region sync vars

        public int Pid { get; }

        [Flags]
        public enum UpdateFlags
        {
            Chat = 0x80,
            InteractEnt = 0x1,
            Appearance = 0x10,
            FacingCoordinate = 0x2,
        }

        public UpdateFlags Flags { get; private set; }

        private void SetFlag(UpdateFlags flag)
            => Flags |= flag;

        [CanBeNull] private ChatMessage _lastChatMessage;
        [CanBeNull] private IWorldEntity _interactingEntity;
        [CanBeNull] private (ushort x, ushort y)? _facingCoordinate;
        [NotNull] private PlayerAppearance _appearance;

        [CanBeNull] public ChatMessage LastChatMessage
        {
            get => _lastChatMessage;
            set
            {
                _lastChatMessage = value;
                SetFlag(UpdateFlags.Chat);
            }
        }
        public IWorldEntity InteractingEntity
        {
            get => _interactingEntity;
            set
            {
                _interactingEntity = value;
                SetFlag(UpdateFlags.InteractEnt);
            }
        }
        [NotNull] public PlayerAppearance Appearance
        {
            get => _appearance;
            set
            {
                // ReSharper disable once ConstantNullCoalescingCondition
                var val = value ?? PlayerAppearance.Default;
                _appearance = val;
                _model.SetAppearance(val);
                SetFlag(UpdateFlags.Appearance);
                IsAppearanceDirty = true;
            }
        }
        public (sbyte x, sbyte y) LastMovedDirection { get; set; } = DirectionHelper.GetDelta(Direction.South);

        [CanBeNull] public (ushort x, ushort y)? FacingCoordinate
        {
            get => _facingCoordinate;
            set
            {
                _facingCoordinate = value;
                if(value != null)
                    SetFlag(UpdateFlags.FacingCoordinate);
            }
        }

        public const int MaxAppearanceUpdateSize = 64;
        public Blob AppearanceUpdateCache { get; set; }= new Blob(MaxAppearanceUpdateSize);
        public bool IsAppearanceDirty { get; set; }

        #endregion

        public bool IsMember => _model.IsMember;
        [NotNull] public string Username => _model.Username;
        [NotNull] public string Password => _model.PasswordHash;
        public byte TitleIcon => _model.TitleIcon;

        [NotNull] public SocketContext Connection { get; }
        [NotNull] public Logger Log => Server.Log;
        public IObservatory Observatory => _observatory;
        private PlayerObservatory _observatory;

        [NotNull] private readonly PlayerModel _model;
        private int _otherPlayerViewRange = MaxViewRange;
        public MovementController Movement { get; }

        public bool NeedsPositionInit { get; private set; } = true;
        public bool TeleportToDestWhenWalking { get; set; }

        /// <summary>
        /// The player cannot see any entities who are further then this many tiles away from the player.
        /// </summary>
        public const int MaxViewRange = 15;

        /// <summary>
        /// The player cannot see any players who are further then this many tiles away from the player.
        /// </summary>
        public int OtherPlayerMaxViewRange
        {
            get => _otherPlayerViewRange;
            set
            {
                var newRange = value.Clamp(0, MaxViewRange);
                if (newRange != _otherPlayerViewRange)
                    Observatory.ReevaluateSightOverride = true;

                _otherPlayerViewRange = newRange;
            }
        }

        /// <exception cref="ArgumentNullException"><paramref name="login"/> is <see langword="null"/></exception>
        public Player([NotNull] NormalPlayerLogin login) 
            : base(login.Server, 
                  login.Server.EntityIdPool)
        {
            if (login == null) throw new ArgumentNullException(nameof(login));

            _model = login.Model;
            Pid = Convert.ToInt32(login.Server.PlayerIdPool.NextId() + 1);
            Connection = new SocketContext(this, login.Server, login.Connection, login.SignlinkUid);

            Position.SetPosition(login.Model.X, login.Model.Y, login.Model.Z);
            _observatory = new PlayerObservatory(this);
            Appearance = new PlayerAppearance(_model);
            Movement = new MovementController(this);

            Connection.SyncMachines.Add(new RegionSyncMachine(Server, Position));
            Connection.SortSyncMachines();

            Server.RegisterNewPlayer(this);

            Connection.SendMessage(new InitializePlayerPacket(this));
            Connection.SendMessage(SetPlayerOptionPacket.Follow);
            Connection.SendMessage(SetPlayerOptionPacket.TradeWith);
            Connection.SendMessage(SetPlayerOptionPacket.Report);
        }

        public void OnMoved()
        {
            FacingCoordinate = null;
        }

        public override void Update(MainLoop loop)
        {
            // sync db model
            _model.SetPosition(Position);

            // reset sync vars
            Flags = 0;
            NeedsPositionInit = false;
            NeedsSightEvaluation = false;
            Movement.MoveUpdate.Reset();

            // reset InteractingEntity if we can't see it anymore.
            if (InteractingEntity != null && !CanSee(InteractingEntity))
                InteractingEntity = null;

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

        public override void SyncTo(ObservableSyncMachine sync, Blob blob, bool isNew)
        {
            if (isNew)
                sync.PushToPlayerSyncMachine(this);
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
            NeedsPositionInit = true;

            Movement.DisposeDirections();
        }

        public void ForceTeleport(ushort x, ushort y)
            => ForceTeleport(x, y, Position.Z);

        protected override void InternalDestroy()
            => Server.UnregisterPlayer(this);

        public override bool CanSee(IWorldEntity obs)
        {
            if (obs.IsDestroyed)
                return false;

            if (obs.Position.Z != Position.Z)
                return false;

            if (!PoE.ContainsEntity(obs))
                return false;

            var range = MaxViewRange;
            if (obs is Player)
                range = OtherPlayerMaxViewRange;

            return obs.Position.MaxDistanceTo(Position) <= range;
        }

        /// <summary>
        /// Sends a system chat message to this player.
        /// </summary>
        public void SendSystemChatMessage(string msg)
            => Connection.SendMessage(new SystemChatMessagePacket(msg));

        public void DebugMsg(string msg, ref bool toggle)
        {
            if(toggle)
                SendSystemChatMessage(msg);
        }

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

        public override string ToString()
        {
            return $"Player \"{Username}\" (UEI: {UniqueEntityId} PID: {Pid})";
        }
    }
}