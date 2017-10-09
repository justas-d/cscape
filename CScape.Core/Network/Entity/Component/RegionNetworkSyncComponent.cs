using System.Diagnostics;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Network.Packet;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Component
{
    /// <summary>
    /// Responsible for syncing current client position region coordinates to the network.
    /// Requires a ClientPositionComponent
    /// </summary>
    [RequiresComponent(typeof(ClientPositionComponent))]
    [RequiresComponent(typeof(NetworkingComponent))]
    public sealed class RegionNetworkSyncComponent : EntityComponent
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

        public RegionNetworkSyncComponent(Game.Entities.Entity parent)
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