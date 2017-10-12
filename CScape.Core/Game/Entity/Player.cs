using System;
using System.Diagnostics;
using CScape.Core.Data;
using CScape.Core.Game.Interface;
using CScape.Core.Game.Interface.Showable;
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



        public const int MaxAppearanceUpdateSize = 64;
        public Blob AppearanceUpdateCache { get; set; } = new Blob(MaxAppearanceUpdateSize);

        public bool IsAppearanceDirty { get; set; }


        public bool IsMember => _model.IsMember;


        [NotNull] private readonly IPlayerModel _model;



        public bool TeleportToDestWhenWalking { get; set; }

        // todo : only register container interfaces if the player can see them
        [NotNull] public BasicItemManager Inventory { get; }
        [NotNull] public EquipmentManager Equipment { get; }
        [NotNull] public PlayerSkills Skills { get; }


        //private readonly IServiceProvider _services;

        public Player([NotNull] IPlayerModel model, ISocketContext socket,
            [NotNull] IServiceProvider services, bool isHighDetail) : base(services)
        {
           // _services = services ?? throw new ArgumentNullException(nameof(services));
            //_model = model ?? throw new ArgumentNullException(nameof(model));

           // Pid = IdPool.NextPlayer();
           // Connection = socket;

          //  _observatory = new PlayerObservatory(services, this);

            //_transform = ObserverClientTransform.Factory.Create(this, _model.X, _model.Y, _model.Z);
           // Transform = _transform;

           // Movement = new MovementController(services, this);
            Interfaces = new PlayerInterfaceController(this);

            Connection.SyncMachines.Add(new RegionSyncMachine(this));
            Connection.SyncMachines.Add(new InterfaceSyncMachine(this));

           // Server.Players.Register(this);

            // send init packets
            Connection.SendPacket(new InitializePlayerPacket(this));
            Connection.SendPacket(SetPlayerOptionPacket.Follow);
            Connection.SendPacket(SetPlayerOptionPacket.TradeWith);
            Connection.SendPacket(SetPlayerOptionPacket.Report);
    
            // set up the sidebar containers
            var ids = _services.ThrowOrGet<IInterfaceIdDatabase>();
            Inventory = new BasicItemManager(ids.BackpackContainer,
                _services, _model.BackpackItems);

            Equipment = new EquipmentManager(ids.EquipmentContainer,
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

            Interface(ids.SkillSidebar, ids.SkillSidebarIdx);
            Interface(ids.QuestSidebar, ids.QuestSidebarIdx);
            Interface(ids.PrayerSidebar, ids.PrayerSidebarIdx);
            // todo : send different spell book interfaces depending on the player's active spellbook
            // todo : keep track of player spellbook state
            Interface(ids.StandardSpellbookSidebar, ids.SpellbookSidebarIdx);
            Interface(ids.FriendsListSidebar, ids.FriendsSidebarIdx);
            Interface(ids.IgnoreListSidebar, ids.IgnoresSidebarIdx);
            Interface(ids.LogoutSidebar, ids.LogoutSidebarIdx);

            Interface(isHighDetail 
                ? ids.OptionsHighDetailSidebar 
                : ids.OptionsLowDetailSidebar,
                ids.OptionsSidebarIdx);

            Interface(ids.ControlsSidebar, ids.ControlsSidebarIdx);

            // container interfaces
            var res = Interfaces.TryShow(new ItemSidebarInterface(ids.BackpackSidebar, ids.BackpackSidebarIdx, Inventory,null));
            Debug.Assert(res, "Cannot show container interface in player ctor ");
            res = Interfaces.TryShow(new ItemSidebarInterface(ids.EquipmentSidebar, ids.EquipmentSidebarIdx, Equipment, null));
            Debug.Assert(res, "Cannot show container interface in player ctor ");

            // setup skills
            var skillSync = new SkillSyncMachine(model.Skills.Experience.Length);
            Connection.SyncMachines.Add(skillSync);
            Skills = new PlayerSkills(services, this, model, skillSync);

            // set update flags
            TickFlags |= UpdateFlags.Appearance;
            IsAppearanceDirty = true;

            // queue for immediate update
            //_services.ThrowOrGet<IMainLoop>().Player.Enqueue(this);
        }

        public void OnMoved()
        {
            //FacingCoordinate = null;
            Interfaces.OnActionOccurred();
        }

        public override void Update(IMainLoop loop)
        {
            // sync db model
            _model.X = Transform.X;
            _model.Y = Transform.Y;
            _model.Z = Transform.Z;

            // reset sync vars
  //          TickFlags = 0;
        //    NeedsPositionInit = false;
            NeedsSightEvaluation = false;
            //Movement.MoveUpdate.Reset();
            
            // reset pressed buttons
            Interfaces.PressedButtonIds.Clear();

//            EntityHelper.TryResetInteractingEntity(this);

            /*
            if (IsDestroyed)
            {
                var msg = $"Updating destroyed player {Username}";
                Log.Warning(this, msg);
                Debug.Fail(msg);
            }
            */

            // check for hard disconnects
            /*
            if(Connection.DeadForMs >= ReapTimeMs)
            {
                Log.Debug(this, $"Reaping {Username}");
                Destroy();
                return;
            }
            */
            /*
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
            */

            //loop.Player.Enqueue(this);
        }
    }
}