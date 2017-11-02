using System.Collections.Generic;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Network.Entity.Flag;
using CScape.Core.Network.Entity.Segment;
using CScape.Core.Network.Packet;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Component
{
    public sealed class NpcNetworkSyncComponent : EntityNetworkSyncComponent
    {
        public override int Priority => (int)ComponentPriority.NpcSync;

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
            writer.SetFlag(new FacingCoordinateUpdateFlag(ent.GetTransform().FacingState));
        }

        protected override void Sync()
        {
            var updates = new List<IUpdateWriter>();

            var sync = GetSyncSegments(updates, f => new NpcUpdateWriter(f));
            var init = CreateNpcInitSegments(updates);

            Parent.AssertGetNetwork().SendPacket(
                new NpcUpdatePacket(
                    sync,
                    init,
                    updates));
        }

        private IEnumerable<IUpdateSegment> CreateNpcInitSegments(List<IUpdateWriter> updates)
        {
            IUpdateWriter UpdateWriterFactory(FlagAccumulatorComponent f)
            {
                return new NpcUpdateWriter(f);
            }

            IUpdateSegment InitSegmentFactory((bool needsUpdate, IEntity entityToBeInitialized) data)
            {
                return new InitNpcSegment(data.entityToBeInitialized.AssertGetNpc(), Parent, data.needsUpdate);
            }

            var init = GetInitSegments(updates, UpdateWriterFactory, InitSegmentFactory);
            return init;
        }
    }
}
