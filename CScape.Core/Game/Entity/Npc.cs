using System;
using CScape.Core.Data;
using CScape.Core.Game.World;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    // todo : health interface
    /// <summary>
    /// Defines an AI pawn NPC.
    /// </summary>
    public class Npc : WorldEntity, IMovingEntity
    {
        #region Sync
        [Flags]
        public enum UpdateFlags : byte
        {
            InteractingEntity = 0x20,
            Text = 1,
            Definition = 2,
            FacingDirection = 4
        }

        public UpdateFlags TickFlags { get; private set; }
        public UpdateFlags PersistFlags { get; private set; }

        public (sbyte x, sbyte y) LastMovedDirection { get; set; }

        private IWorldEntity _interactingEntity;

        public IWorldEntity InteractingEntity
        {
            get => _interactingEntity;
            set
            {
                _interactingEntity = value;
                PersistFlags |= UpdateFlags.InteractingEntity;
            }
        }

        [CanBeNull] public string LastSentTextMessage { get; private set; }

        public bool NeedsPositionInit { get; private set; } = true;
        #endregion

        public int ViewRange { get; set; } = 15;
        public MovementController Movement { get; }

        private short _definition;
        public short NpcDefinitionId
        {
            get => _definition;
            set
            {
                if (_definition == value) return;

                _definition = value;
                TickFlags |= UpdateFlags.Definition;
            }
        }
        public short UniqueNpcId { get; }

        public Npc(IServiceProvider service,
            short npcDefId,
            IPosition pos,
            PlaneOfExistance poe = null) : this(service, npcDefId, pos.X, pos.Y, pos.Z, poe) { }

        public Npc(IServiceProvider service,
            short npcDefId,
            int x, int y, byte z,
            PlaneOfExistance poe = null) : base(service)
        {
            if (0 > npcDefId) throw new ArgumentOutOfRangeException(nameof(npcDefId));

            Transform = new ServerTransform(this, x, y, z, poe);
            Movement = new MovementController(service, this);

            _definition = npcDefId;
            UniqueNpcId = IdPool.NextNpc();

            // queue for immediate update
            service.ThrowOrGet<IMainLoop>().Npc.Enqueue(this);

            // register
            Server.Npcs.Register(this);
        }

        /// <summary>
        /// Forces the entity to say, above their head overhead, the given text.
        /// </summary>
        public void Say(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            LastSentTextMessage = text;
            TickFlags |= UpdateFlags.Text;
        }

        protected override void InternalDestroy()
        {
            IdPool.FreeNpc(UniqueNpcId);
            Server.Npcs.Unregister(this);
        }

        public void OnMoved()
        {
            // ignored, todo : reset npc facing coordinate?
        }

        public override void Update(IMainLoop loop)
        {
            // reset update flags
            NeedsPositionInit = false;
            TickFlags = 0;
            LastSentTextMessage = null;

            EntityHelper.TryResetInteractingEntity(this);

            // reset persist InteractingEntity flag
            if (InteractingEntity == null)
                PersistFlags &= ~UpdateFlags.InteractingEntity;

            // todo : destroy if 0 > health

            loop.Npc.Enqueue(this);
        }


        public override void SyncTo(ObservableSyncMachine sync, Blob blob, bool isNew)
        {
            if(isNew) sync.NpcSync.PushNpc(this);
        }

        public override bool CanSee(IWorldEntity ent)
        {
            if (ent.IsDestroyed)
                return false;

            if (ent.Transform.Z != Transform.Z)
                return false;

            if (!Transform.PoE.ContainsEntity(ent))
                return false;

            return ent.Transform.MaxDistanceTo(Transform) <= ViewRange;
        }
    }
}