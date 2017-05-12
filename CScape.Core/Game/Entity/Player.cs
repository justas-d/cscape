using System;
using System.Diagnostics;
using CScape.Core.Data;
using CScape.Core.Game.Interface;
using CScape.Core.Game.World;
using CScape.Core.Injection;
using CScape.Core.Network;
using CScape.Core.Network.Packet;
using CScape.Core.Network.Sync;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    //todo: change username feature
    //todo: change password feature

    /// <summary>
    /// Defines a player entity that exists in the world.
    /// </summary>
    public sealed class Player : WorldEntity, IMovingEntity, IObserver
    {
        #region debug vars

        public bool DebugItems = true;
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
                    _debugStatSync = new DebugStatSyncMachine(_services);
                    Connection.SyncMachines.Add(_debugStatSync);
                    Connection.SortSyncMachines();
                }

                _debugStatSync.IsEnabled = value;
            }
        }

        private DebugStatSyncMachine _debugStatSync;

        #endregion

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

        private void SetFlag(UpdateFlags flag)
            => Flags |= flag;

        [CanBeNull] private ChatMessage _lastChatMessage;
        [CanBeNull] private IWorldEntity _interactingEntity;
        [CanBeNull] private (ushort x, ushort y)? _facingCoordinate;
        
        [CanBeNull] public ChatMessage LastChatMessage
        {
            get => _lastChatMessage;
            set
            {
                _lastChatMessage = value;
                SetFlag(UpdateFlags.Chat);
            }
        }
        [CanBeNull] public (ushort x, ushort y)? FacingCoordinate
        {
            get => _facingCoordinate;
            set
            {
                _facingCoordinate = value;
                if (value != null)
                    SetFlag(UpdateFlags.FacingCoordinate);
            }
        }

        [NotNull] public IPlayerAppearance Appearance
        {
            get => _model.Appearance;
            set
            {
                var val = value ?? throw new InvalidOperationException("Player appearance cannot be null.");
                _model.Appearance = val;

                SetFlag(UpdateFlags.Appearance);
                IsAppearanceDirty = true;
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

        public const int MaxAppearanceUpdateSize = 64;
        public Blob AppearanceUpdateCache { get; set; } = new Blob(MaxAppearanceUpdateSize);

        /// <summary>
        /// If set, will invalidate appearance update caches.
        /// </summary>
        public bool IsAppearanceDirty { get; set; }

        [NotNull] public RegionSyncMachine RegionSync { get; }

        public bool NeedsPositionInit { get; private set; } = true;
        public short Pid { get; }
        public (sbyte x, sbyte y) LastMovedDirection { get; set; } = DirectionHelper.GetDelta(Direction.South);


        #endregion

        [NotNull] public string Username => _model.Id;
        [NotNull] public string Password => _model.PasswordHash;
        public byte TitleIcon => _model.TitleIcon;
        public bool IsMember => _model.IsMember;

        [NotNull] public SocketContext Connection { get; }
        public IObservatory Observatory => _observatory;
        private readonly PlayerObservatory _observatory;

        [NotNull] private readonly IPlayerModel _model;
        private int _otherPlayerViewRange = MaxViewRange;

        public MovementController Movement { get; }

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

        [NotNull] public InterfacedItemManager Inventory { get; }
        [NotNull] public IInterfaceManager Interfaces { get; }

        private readonly IPlayerIdPool _playerIdPool;
        private readonly IServiceProvider _services;

        /// <exception cref="ArgumentNullException"><paramref name="login"/> is <see langword="null"/></exception>
        public Player([NotNull] NormalPlayerLogin login) : base(login.Service)
        {
            if (login == null) throw new ArgumentNullException(nameof(login));

            _services = login.Service;
            _model = login.Model;
            _playerIdPool = login.Service.ThrowOrGet<IPlayerIdPool>();

            Pid = _playerIdPool.NextId();
            Connection = new SocketContext(login.Service, this, login.Connection, login.SignlinkUid);

            _observatory = new PlayerObservatory(this);

            Transform = ObserverTransform.Factory.Create(this, login.Model.X, login.Model.Y, login.Model.Z);
            Movement = new MovementController(login.Service, this);
            Interfaces = new PlayerInterfaceController(this);

            RegionSync = new RegionSyncMachine(this);
            Connection.SyncMachines.Add(RegionSync);
            Connection.SyncMachines.Add(new InterfaceSyncMachine(this));

            Connection.SortSyncMachines();

            Server.RegisterPlayer(this);

            Connection.SendMessage(new InitializePlayerPacket(this));
            Connection.SendMessage(SetPlayerOptionPacket.Follow);
            Connection.SendMessage(SetPlayerOptionPacket.TradeWith);
            Connection.SendMessage(SetPlayerOptionPacket.Report);
    
            // set up the rest of the sidebar interfaces
            Inventory = new InterfacedItemManager(login.Service, _model.BackpackItems,
                InterfaceConstants.PlayerBackpackInventoryId);

            Interfaces.TryRegister(Inventory);
            Interfaces.TryShow(new ItemSidebarInterface(InterfaceConstants.PlayerBackpackInterfaceId, 3, Inventory,null));

            SetFlag(UpdateFlags.Appearance);
            IsAppearanceDirty = true;
        }

        public void OnMoved()
        {
            FacingCoordinate = null;
            Interfaces.OnActionOccurred();
        }

        public override void Update(IMainLoop loop)
        {
            // sync db model
            _model.X = Transform.X;
            _model.Y = Transform.Y;
            _model.Z = Transform.Z;

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
        public void ForceTeleport(int x, int y, byte z)
        {
            if (Transform.X == x && Transform.Y == y && Transform.Z == z)
                return;

            Transform.Teleport(x,y,z);
            NeedsPositionInit = true;

            Movement.DisposeDirections();
        }

        public void ForceTeleport(int x, int y)
            => ForceTeleport(x, y, Transform.Z);

        protected override void InternalDestroy()
        {
            _playerIdPool.FreeId(Pid);
            Server.UnregisterPlayer(this);
        }

        public override bool CanSee(IWorldEntity obs)
        {
            if (obs.IsDestroyed)
                return false;

            if (obs.Transform.Z != Transform.Z)
                return false;

            if (!Transform.PoE.ContainsEntity(obs))
                return false;

            var range = MaxViewRange;
            if (obs is Player)
                range = OtherPlayerMaxViewRange;

            return obs.Transform.MaxDistanceTo(Transform) <= range;
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