using System.Diagnostics;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using CScape.Core.Network.Packet;
using JetBrains.Annotations;
using CScape.Core.Game.Entities.Interface;

namespace CScape.Core.Game.Entities.Component
{
    /// <summary>
    /// Responsible for syncing current client position region coordinates to the network.
    /// Requires a ClientPositionComponent
    /// </summary>
    [RequiresFragment(typeof(ClientPositionComponent))]
    [RequiresFragment(typeof(NetworkingComponent))]
    public sealed class RegionSyncNetFragment : EntityComponent
    {
        public override int Priority { get; } = ComponentConstants.PriorityRegion;

        public bool ShouldSendSystemMessageWhenSyncing { get; set; }

        [NotNull]
        private ClientPositionComponent Pos
        {
            get
            {
                var val = Parent.Components.Get<ClientPositionComponent>();
                Debug.Assert(val != null);
                return val;
            }
        }

        [NotNull]
        private NetworkingComponent Net
        {
            get
            {
                var val = Parent.Components.Get<NetworkingComponent>();
                Debug.Assert(val != null);
                return val;
            }
        }

        public RegionSyncNetFragment(Entity parent)
            :base(parent)
        {
            
        }

        private void SyncRegion((int x, int y) pos)
        {
            if (ShouldSendSystemMessageWhenSyncing)
            {
                Parent.SystemMessage($"Sync region: {pos.x} + 6 {pos.y} + 6");
            }

            Net.SendPacket(
                new SetRegionCoordinate(
                    (short)(pos.x + 6),
                    (short)(pos.y + 6)));
        }

        public override void ReceiveMessage(EntityMessage msg)
        {
            switch (msg.Event)
            {
                case EntityMessage.EventType.ClientRegionChanged:
                {
                    SyncRegion(msg.AsClientRegionChanged());
                    break;
                }
                case EntityMessage.EventType.NetworkReinitialize:
                {

                    SyncRegion(Pos.ClientRegion);
                    break;
                }
            }
        }
    }
}