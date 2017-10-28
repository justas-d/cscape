using System.Collections.Generic;
using CScape.Core.Extensions;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Network.Entity.Flag;
using CScape.Core.Network.Packet;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Component
{
    public sealed class NpcNetworkSyncComponent : EntityNetworkSyncComponent
    {
        public override int Priority => (int)SyncComponentPriority.Npc;

        public NpcNetworkSyncComponent([NotNull] IEntity parent) : base(parent)
        {
        }

        protected override bool IsHandleableEntity(IEntityHandle h)
        {
            if (h.IsDead())
                return false;

            return h.Get().Components.Get<NpcComponent>() != null;
        }

        protected override void SetInitialFlags(IUpdateWriter writer, IEntity ent)
        {
            writer.SetFlag(new InteractingEntityUpdateFlag(ent.GetTransform().InteractingEntity));
            writer.SetFlag(new FacingCoordinateUpdateFlag(ent.GetTransform().FacingData));
        }

        protected override void Sync()
        {
            var updates = new List<IUpdateWriter>();

            var sync = GetSyncSegments(updates, f => new NpcUpdateWriter(f));
            var init = GetInitSegments(updates, f => new NpcUpdateWriter(f));

            Parent.AssertGetNetwork().SendPacket(
                new NpcUpdatePacket(
                    sync,
                    init,
                    updates));
        }
    }
}
