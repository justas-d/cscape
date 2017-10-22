using CScape.Core.Extensions;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Entities.Message;
using CScape.Core.Network.Packet;
using CScape.Models.Extensions;
using CScape.Models.Game;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Message;

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

        public RegionNetworkSyncComponent(Game.Entities.Entity parent)
            :base(parent)
        {
            
        }

        private void SyncRegion(IPosition pos)
        {
            if (ShouldSendSystemMessageWhenSyncing)
            {
                Parent.SystemMessage($"Sync region: {pos.X} + 6 {pos.Y} + 6", SystemMessageFlags.Debug | SystemMessageFlags.Network);
            }

            Parent.AssertGetNetwork().SendPacket(
                new SetRegionCoordinate(
                    (short)(pos.X + 6),
                    (short)(pos.Y + 6)));
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case (int)MessageId.ClientRegionChanged:
                {
                    SyncRegion(msg.AsClientRegionChange().Position);
                    break;
                }
                case (int)MessageId.NetworkReinitialize:
                {
                    SyncRegion(Parent.AssertGetClientPosition().ClientRegion);
                    break;
                }
            }
        }
    }
}