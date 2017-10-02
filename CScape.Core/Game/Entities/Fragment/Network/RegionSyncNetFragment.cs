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

        public void ReceiveMessage(EntityMessage msg)
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

        public void Update(IMainLoop loop, NetworkingComponent network)
        {

        }
    }
}