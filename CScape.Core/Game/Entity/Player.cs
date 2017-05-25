using System;
using System.Diagnostics;
using CScape.Core.Data;
using CScape.Core.Game.Interface;
using CScape.Core.Game.Interface.Showable;
using CScape.Core.Game.World;
using CScape.Core.Injection;
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
    public sealed class Player 
        : WorldEntity, IMovingEntity, IObserver, IDamageable, IEquatable<Player>
    {
        #region debug vars

        public bool DebugEntitySync = true;
        public bool DebugItems = false;
        public bool DebugCommands = false;
        public bool DebugPackets = false;
        public bool DebugRegion = false;
        public bool DebugStats
        {
            get => _debugStatSync?.IsEnabled ?? false;
            set
            {
                if (value && _debugStatSync == null)
                {
                    _debugStatSync = new DebugStatSyncMachine(_services);
                    Connection.SyncMachines.Add(_debugStatSync);
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
            ForcedMovement = 0x400,
            ParticleEffect = 0x100,
            Animation = 8,
            ForcedText = 4,
            Chat = 0x80,
            InteractEnt = 0x1,
            Appearance = 0x10,
            FacingCoordinate = 0x2,
            PrimaryHit = 0x20,
            SecondaryHit = 0x200,
        }

        /// <summary>
        /// Reset every tick
        /// </summary>
        public UpdateFlags TickFlags { get; private set; }

        [CanBeNull] private ChatMessage _lastChatMessage;
        [CanBeNull] private IWorldEntity _interactingEntity;
        [CanBeNull] private (ushort x, ushort y)? _facingCoordinate;

        private  ForcedMovement _forceMovement;

        public ForcedMovement ForcedMovement
        {
            get => _forceMovement;
            set
            {
                TickFlags |= UpdateFlags.ForcedMovement;
                ForcedMovement = value;
            }
        }

        private ParticleEffect _effect;
        public ParticleEffect Effect
        {
            get => _effect;
            set
            {
                TickFlags |= UpdateFlags.ParticleEffect;
                _effect = value;
            }
        }

        private Animation _animData;
        public Animation Animation
        {
            get => _animData;
            set
            {
                TickFlags |= UpdateFlags.Animation;
                _animData = value;
            }
        }

        [CanBeNull] public ChatMessage LastChatMessage
        {
            get => _lastChatMessage;
            set
            {
                _lastChatMessage = value;
                TickFlags |= UpdateFlags.Chat;
            }
        }

        // todo : maybe expose this via some interface? or move it to WorldEntity?
        [CanBeNull] public (ushort x, ushort y)? FacingCoordinate
        {
            get => _facingCoordinate;
            set
            {
                _facingCoordinate = value;
                if (value != null)
                    TickFlags |= UpdateFlags.FacingCoordinate;
            }
        }

        [NotNull] public IPlayerAppearance Appearance
        {
            get => _model.Appearance;
            set
            {
                var val = value ?? throw new InvalidOperationException("Player appearance cannot be null.");
                _model.Appearance = val;

                TickFlags |= UpdateFlags.Appearance;
                IsAppearanceDirty = true;
            }
        }

        public IWorldEntity InteractingEntity
        {
            get => _interactingEntity;
            set
            {
                _interactingEntity = value;
                TickFlags |= UpdateFlags.InteractEnt;
            }
        }

        private string _forcedText;
        public string ForcedText
        {
            get => _forcedText;
            set
            {
                _forcedText = value;
                TickFlags |= UpdateFlags.ForcedText;
            }
        }

        public const int MaxAppearanceUpdateSize = 64;
        public Blob AppearanceUpdateCache { get; set; } = new Blob(MaxAppearanceUpdateSize);

        /// <summary>
        /// If set, will invalidate appearance update caches.
        /// </summary>
        public bool IsAppearanceDirty { get; set; }

        public bool NeedsPositionInit { get; private set; } = true;
        public short Pid { get; }
        public (sbyte x, sbyte y) LastMovedDirection { get; set; } = DirectionHelper.GetDelta(Direction.South);

        #endregion

        [NotNull] public string Username => _model.Id;
        [NotNull] public string Password => _model.PasswordHash;
        public byte TitleIcon => _model.TitleIcon;
        public bool IsMember => _model.IsMember;

        [NotNull] public ISocketContext Connection { get; }
        public IObservatory Observatory => _observatory;
        [NotNull] private readonly ClientTransform _transform;
        [NotNull] public IClientTransform ClientTransform => _transform;
        private readonly PlayerObservatory _observatory;

        [NotNull] private readonly IPlayerModel _model;
        private int _otherPlayerViewRange = MaxViewRange;

        public MovementController Movement { get; }

        public bool TeleportToDestWhenWalking { get; set; }
        
        /// <summary>
        /// The player cannot see any entities who are further then this many tiles away from the player.
        /// </summary>
        public const int MaxViewRange = 15;

        // todo : lower player ViewRange if out buffer is close to being overrun.
        /// <summary>
        /// The player cannot see any entities who are further then this many tiles away from the player.
        /// </summary>
        public int ViewRange
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

        public HitData SecondaryHit { get; private set; }
        public HitData PrimaryHit { get; private set; }

        // todo : hook up Player.MaxHealth to player skills
        public byte MaxHealth { get; set; } = 10;
        public byte CurrentHealth
        {
            get => _model.Health;
            private set => _model.Health = value;
        } 

        public bool Damage(byte dAmount, HitType type, bool secondary)
        {
            var hit = HitData.Calculate(this, type, dAmount);
            CurrentHealth = hit.CurrentHealth;

            if (secondary)
            {
                SecondaryHit = hit;
                TickFlags |= UpdateFlags.SecondaryHit;
            }
            else
            {
                PrimaryHit = hit;
                TickFlags |= UpdateFlags.PrimaryHit;
            }

            return CurrentHealth == 0;
            // todo : handle player death
        }

        // todo : only register container interfaces if the player can see them
        [NotNull] public BasicItemManager Inventory { get; }
        [NotNull] public EquipmentManager Equipment { get; }

        [NotNull] public IInterfaceManager Interfaces { get; }

        private readonly IServiceProvider _services;

        public Player([NotNull] IPlayerModel model, ISocketContext socket,
            [NotNull] IServiceProvider services, bool isHighDetail) : base(services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _model = model ?? throw new ArgumentNullException(nameof(model));

            Pid = IdPool.NextPlayer();
            Connection = socket;

            _observatory = new PlayerObservatory(services, this);

            _transform = Entity.ObserverClientTransform.Factory.Create(this, _model.X, _model.Y, _model.Z);
            Transform = _transform;

            Movement = new MovementController(services, this);
            Interfaces = new PlayerInterfaceController(this);

            Connection.SyncMachines.Add(new RegionSyncMachine(this));
            Connection.SyncMachines.Add(new InterfaceSyncMachine(this));

            Server.Players.Register(this);

            // send init packets
            Connection.SendPacket(new InitializePlayerPacket(this));
            Connection.SendPacket(SetPlayerOptionPacket.Follow);
            Connection.SendPacket(SetPlayerOptionPacket.TradeWith);
            Connection.SendPacket(SetPlayerOptionPacket.Report);
    
            // set up the sidebar containers
            var ids = _services.ThrowOrGet<IInterfaceIdDatabase>();
            Inventory = new BasicItemManager(ids.BackpackInventory,
                _services, _model.BackpackItems);

            Equipment = new EquipmentManager(ids.EquipmentInventory,
                this, _services, _model.Equipment);

            // register sidebar containers
            Interfaces.TryRegister(Inventory);
            Interfaces.TryRegister(Equipment);

            // sidebar interfaces
            void Interface(int id, int idx, IButtonHandler handler = null)
            {
                var result = Interfaces.TryShow(new BasicSidebarInterface(id, idx, handler));
                Debug.Assert(result, $"Interfaces.TryShow id {id} idx {idx} ret false");
            }

            Interface(ids.SkillSidebarInterface, ids.SkillSidebarIdx);
            Interface(ids.QuestSidebarInterface, ids.QuestSidebarIdx);
            Interface(ids.PrayerSidebarInterface, ids.PrayerSidebarIdx);
            // todo : send different spell book interfaces depending on the player's active spellbook
            // todo : keep track of player spellbook state
            Interface(ids.StandardSpellbookSidebarInterface, ids.SpellbookSidebarIdx);
            Interface(ids.FriendsListSidebarInterface, ids.FriendsSidebarIdx);
            Interface(ids.IgnoreListSidebarInterface, ids.IgnoresSidebarIdx);
            Interface(ids.LogoutSidebarInterface, ids.LogoutSidebarIdx);

            if(isHighDetail)
                Interface(ids.OptionsHighDetailSidebarInterface, ids.OptionsSidebarIdx);
            else
                Interface(ids.OptionsLowDetailSidebarInterface, ids.OptionsSidebarIdx);

            Interface(ids.ControlsSidebarInterface, ids.ControlsSidebarIdx);

            // container interfaces
            var res = Interfaces.TryShow(new ItemSidebarInterface(ids.BackpackSidebarInterface, ids.BackpackSidebarIdx, Inventory,null));
            Debug.Assert(res, "Cannot show container interface in player ctor ");
            res = Interfaces.TryShow(new ItemSidebarInterface(ids.EquipmentSidebarInterface, ids.EquipmentSidebarIdx, Equipment, null));
            Debug.Assert(res, "Cannot show container interface in player ctor ");

            // set update flags
            TickFlags |= UpdateFlags.Appearance;
            IsAppearanceDirty = true;

            // queue for immediate update
            _services.ThrowOrGet<IMainLoop>().Player.Enqueue(this);
        }

        public void OnMoved()
        {
            FacingCoordinate = null;
            Interfaces.OnActionOccurred();

            SendSystemChatMessage($"{ClientTransform.ClientRegion.x} {ClientTransform.ClientRegion.y}");
        }

        /// <summary>
        /// In milliseconds, the delay between a socket dying and it's player being removed
        /// from the world. todo Default: 60 seconds.
        /// </summary>
        public long ReapTimeMs { get; set; } = 1000 * 60;

        public override void Update(IMainLoop loop)
        {
            // sync db model
            _model.X = Transform.X;
            _model.Y = Transform.Y;
            _model.Z = Transform.Z;

            // reset sync vars
            TickFlags = 0;
            NeedsPositionInit = false;
            NeedsSightEvaluation = false;
            Movement.MoveUpdate.Reset();

            EntityHelper.TryResetInteractingEntity(this);

            if (IsDestroyed)
            {
                var msg = $"Updating destroyed player {Username}";
                Log.Warning(this, msg);
                Debug.Fail(msg);
            }

            // check for hard disconnects
            if(Connection.DeadForMs >= ReapTimeMs)
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
            IdPool.FreePlayer(Pid);
            Server.Players.Unregister(this);
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
                range = ViewRange;

            return obs.Transform.MaxDistanceTo(Transform) <= range;
        }

        /// <summary>
        /// Sends a system chat message to this player.
        /// </summary>
        public void SendSystemChatMessage(string msg) 
            => Connection.SendPacket(new SystemChatMessagePacket(msg));

        public void DebugMsg(string msg, ref bool toggle)
        {
            if(toggle) SendSystemChatMessage(msg);
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

        /// <summary>
        /// Indicates whether this player is equal to any other player based on the equality of their unique <see cref="Username"/>
        /// </summary>
        public bool Equals(Player other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Username.Equals(other.Username, StringComparison.OrdinalIgnoreCase);
        }
    }
}