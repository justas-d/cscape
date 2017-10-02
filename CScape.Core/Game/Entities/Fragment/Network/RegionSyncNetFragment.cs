using System.Diagnostics;
using CScape.Core.Game.Entities.Fragment.Component;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using CScape.Core.Network.Packet;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Fragment.Network
{
    /// <summary>
    /// Responsible for syncing current client position region coordinates to the network.
    /// Requires a ClientPositionComponent
    /// </summary>
    [RequiresFragment(typeof(ClientPositionComponent))]
    public sealed class RegionSyncNetFragment : IEntityNetFragment
    {
        public Entity Parent { get; }
        public int Priority { get; } = NetFragConstants.PriorityRegion;

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
        {
            Parent = parent;
        }

        private void SyncRegion()
        {
            if (ShouldSendSystemMessageWhenSyncing)
            {
                Parent.SystemMessage($"Sync region: {Pos.ClientRegion.x} + 6 {Pos.ClientRegion.y} + 6");
            }

            Net.SendPacket(
                new SetRegionCoordinate(
                    (short)(Pos.ClientRegion.x + 6),
                    (short)(Pos.ClientRegion.y + 6)));
        }

        public void ReceiveMessage(EntityMessage msg)
        {
            switch (msg.Event)
            {
                case EntityMessage.EventType.ClientRegionChanged:
                {
                    SyncRegion();
                    break;
                }
                case EntityMessage.EventType.NetworkReinitialize:
                {
                    SyncRegion();
                    break;
                }
            }
        }

        public void Update(IMainLoop loop, NetworkingComponent network)
        {

        }
    }
}