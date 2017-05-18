using System;
using CScape.Core.Data;
using CScape.Core.Game.World;
using CScape.Core.Injection;

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
        public enum UpdateFlags
        {
            
        }

        public UpdateFlags TickFlags { get; private set; }
        public UpdateFlags PersistFlags { get; private set; }

        public (sbyte x, sbyte y) LastMovedDirection { get; set; }

        // todo set InteractingEntity flag in npc
        public IWorldEntity InteractingEntity { get; set; }

        public bool NeedsPositionInit { get; private set; } = true;
        #endregion

        public int ViewRange { get; set; } = 15;
        public MovementController Movement { get; }

        public int NpcDefinitionId { get; }
        public short UniqueNpcId { get; }

        public Npc(IServiceProvider service,
            int npcDefId,
            IPosition pos,
            PlaneOfExistance poe = null) : this(service, npcDefId, pos.X, pos.Y, pos.Z, poe) { }

        public Npc(IServiceProvider service,
            int npcDefId,
            int x, int y, byte z,
            PlaneOfExistance poe = null) : base(service)
        {
            if (0 > npcDefId) throw new ArgumentOutOfRangeException(nameof(npcDefId));

            Transform = new ServerTransform(this, x, y, z, poe);
            Movement = new MovementController(service, this);

            NpcDefinitionId = npcDefId;
            UniqueNpcId = IdPool.NextNpc();

            // todo : npc init
        }

        protected override void InternalDestroy()
        {
            IdPool.FreeNpc(UniqueNpcId);
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

            // todo : destroy if 0 > health
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