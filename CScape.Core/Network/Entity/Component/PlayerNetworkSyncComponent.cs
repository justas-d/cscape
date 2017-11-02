using System.Collections.Generic;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Network.Entity.Flag;
using CScape.Core.Network.Entity.Segment;
using CScape.Core.Network.Packet;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;

namespace CScape.Core.Network.Entity.Component
{
    /// <summary>
    /// Responsible for syncing every visible player entity to the network.
    /// </summary>
    [RequiresComponent(typeof(NetworkingComponent))]
    [RequiresComponent(typeof(PlayerComponent))]
    public sealed class PlayerNetworkSyncComponent: EntityNetworkSyncComponent
    {
        public override int Priority => (int)ComponentPriority.PlayerSync;
        
        public PlayerNetworkSyncComponent(
            IEntity parent)
            :base(parent)
        {
            
        }

        protected override bool IsHandleableEntity(IEntityHandle h)
        {
            if (h.IsDead())
                return false;

            return h.Get().GetPlayer() != null;
        }

        protected override void SetInitialFlags(IUpdateWriter writer, IEntity ent)
        {
            var flags = ent.Components.AssertGet<FlagAccumulatorComponent>();
            writer.SetFlag(new PlayerAppearanceUpdateFlag(flags.AppearanceCache));
            writer.SetFlag(new FacingCoordinateUpdateFlag(ent.GetTransform().FacingState));
            writer.SetFlag(new InteractingEntityUpdateFlag(ent.GetTransform().InteractingEntity));
        }

        protected override void Sync()
        {
            var updates = new List<IUpdateWriter>();

            var local = CreateUpdateSegmentForParent(updates);

            var sync = GetSyncSegments(updates, f => new PlayerUpdateWriter(f));
            var init = CreatePlayerInitSegments(updates);

            /* Send packet */
            Parent.Components.AssertGet<NetworkingComponent>().SendPacket(
                new PlayerUpdatePacket(
                    local,
                    sync,
                    init,
                    updates));
        }

        private IEnumerable<IUpdateSegment> CreatePlayerInitSegments(List<IUpdateWriter> updates)
        {
            IUpdateWriter UpdateWriterFactory(FlagAccumulatorComponent f) => new PlayerUpdateWriter(f);

            IUpdateSegment InitSegmentFactory((bool needsUpdate, IEntity entityToBeInitialized) data)
                => new InitPlayerSegment(data.entityToBeInitialized.AssertGetPlayer(), Parent.AssertGetPlayer(),
                    data.needsUpdate);

            var init = GetInitSegments(updates,
                UpdateWriterFactory,
                InitSegmentFactory);

            return init;
        }

        private IUpdateSegment CreateUpdateSegmentForParent(List<IUpdateWriter> updates)
        {
            IUpdateSegment local;

            var flags = Parent.Components.AssertGet<FlagAccumulatorComponent>();
            var updater = new LocalPlayerUpdateWriter(flags);
            var needsUpdate = updater.NeedsUpdate();

            if (flags.Reinitialize)
            {
                local = new LocalPlayerInitSegment(Parent.AssertGetPlayer(), needsUpdate);
            }
            else
                local = CommonSegmentResolve(flags, needsUpdate);

            if (needsUpdate)
                updates.Add(updater);

            return local;
        }
    }
}