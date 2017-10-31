using System.Collections.Generic;
using System.Linq;
using CScape.Core.Extensions;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Network.Packet;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Component
{
    [RequiresComponent(typeof(NetworkingComponent))]
    [RequiresComponent(typeof(ClientPositionComponent))]
    public sealed class GroundItemNetworkSyncComponent : EntityComponent
    {
        public override int Priority => (int)ComponentPriority.GroundItemSync;

        private NetworkingComponent Network => Parent.Components.AssertGet<NetworkingComponent>();
        private ClientPositionComponent ClientPos => Parent.Components.AssertGet<ClientPositionComponent>();

        // holds packets sorted by region
        private readonly Dictionary<(int x, int y), List<BaseGroundObjectPacket>> _buckets
            = new Dictionary<(int x, int y), List<BaseGroundObjectPacket>>();

        public GroundItemNetworkSyncComponent([NotNull] IEntity parent) : base(parent)
        {
        }

        private ((int x, int y) regionGrid, (int x, int y) offset) GetLocalCoords(
            IGroundItemComponent item)
        {
            var t = item.Parent.GetTransform();
            var client = ClientPos;

            // get item local coords from the perspective of the player's client transform
            var itemLocal =
                (t.X - client.Base.X,
                t.Y - client.Base.Y);

            // calc the offset of the item in the 8x8 region it belongs to 
            var offset = (
                itemLocal.Item1 % 8,
                itemLocal.Item2 % 8);

            // calc item's region locals
            var region = (
                itemLocal.Item1 - offset.Item1,
                itemLocal.Item2 - offset.Item2);

            return (region, offset);
        }

        private void AddPacket(BaseGroundObjectPacket packet, (int x, int y) regionGrid)
        {
            // lazy init the bucket
            if (!_buckets.ContainsKey(regionGrid))
                _buckets.Add(regionGrid, new List<BaseGroundObjectPacket>());

            _buckets[regionGrid].Add(packet);

            // log invalid offsets
            if (packet.IsInvalid)
                Parent.SystemMessage($"Invalid spawn ground item packet at offsets: ({regionGrid.x} {regionGrid.y})", 
                    CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Network);
        }

        private void RemoveItem(IGroundItemComponent item)
        {
            var coords = GetLocalCoords(item);
            AddPacket(new DeleteGroundItemPacket(item.Item.Id.ItemId -1, coords.offset), coords.regionGrid);
        }

        private void NewItem(IGroundItemComponent item)
        {
            var coords = GetLocalCoords(item);
            AddPacket(new SpawnGroundItemPacket(item.Item, coords.offset), coords.regionGrid);
        }

        private void UpdateItemAmount(GroundItemMessage item)
        {
            var coords = GetLocalCoords(item.Item);
            AddPacket(new UpdateGroundItemAmountPacket(item.After, item.Before.Amount, coords.offset), coords.regionGrid);
        }

        private void Sync()
        {
            var net = Network;
            foreach (var kvp in _buckets)
            {
                // dont sync empty bucket
                if (!kvp.Value.Any())
                    continue;

                // sync bucket
                net.SendPacket(new EmbeddedRegionGroundObjectWrapperPacket(kvp.Key, kvp.Value));

                kvp.Value.Clear();
            }
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            IGroundItemComponent GetItem(IEntityHandle h)
            {
                if (h.IsDead()) return null;
                return h.Get().GetGroundItem();
            }

            switch (msg.EventId)
            {
                case (int)MessageId.NetworkPrepare:
                {
                    Sync();
                    break;
                }
                case (int)MessageId.GroundItemAmountUpdate:
                {
                    UpdateItemAmount(msg.AsGroundItemAmountUpdate());
                    break;
                }

                case (int)MessageId.EntityEnteredViewRange:
                {
                    var i = GetItem(msg.AsEntityEnteredViewRange().Entity);
                    if (i == null) break;
                    NewItem(i);
                    break;
                }
                case (int)MessageId.EntityLeftViewRange:
                {
                    var i = GetItem(msg.AsEntityLeftViewRange().Entity);
                    if (i == null) break;
                    RemoveItem(i);
                    break;
                }
            }
        }
    }
}
