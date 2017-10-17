using System.Collections.Generic;
using System.Linq;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Entities.Message;
using CScape.Core.Network.Packet;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Component
{
    [RequiresComponent(typeof(NetworkingComponent))]
    [RequiresComponent(typeof(ClientPositionComponent))]
    public sealed class GroundItemNetworkSync : EntityComponent
    {
        public override int Priority { get; }

        private NetworkingComponent Network => Parent.Components.AssertGet<NetworkingComponent>();
        private ClientPositionComponent ClientPos => Parent.Components.AssertGet<ClientPositionComponent>();

        // holds packets sorted by region
        private readonly Dictionary<(int x, int y), List<BaseGroundObjectPacket>> _buckets
            = new Dictionary<(int x, int y), List<BaseGroundObjectPacket>>();


        public GroundItemNetworkSync([NotNull] Game.Entities.Entity parent) : base(parent)
        {
        }

        private ((int x, int y) regionGrid, (int x, int y) offset) GetLocalCoords(
            GroundItemComponent item)
        {
            var t = item.Parent.GetTransform();
            var client = ClientPos;

            // get item local coords from the perspective of the player's client transform
            var itemLocal =
                (t.X - client.Base.x,
                t.Y - client.Base.y);

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
                Parent.SystemMessage($"Invalid spawn ground item packet at offsets: ({regionGrid.x} {regionGrid.y})");
        }

        private void RemoveItem(GroundItemComponent item)
        {
            var coords = GetLocalCoords(item);
            AddPacket(new DeleteGroundItemPacket(item.Item.Id.ItemId, coords.offset), coords.regionGrid);
        }

        private void NewItem(GroundItemComponent item)
        {
            var coords = GetLocalCoords(item);
            AddPacket(new SpawnGroundItemPacket(item.Item, coords.offset), coords.regionGrid);
        }

        private void UpdateItemAmount(GroundItemChangeMetadata item)
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

        public override void ReceiveMessage(GameMessage msg)
        {
            GroundItemComponent GetItem(EntityHandle h)
            {
                if (h.IsDead()) return null;
                return h.Get().Components.Get<GroundItemComponent>();
            }

            switch (msg.Event)
            {
                case GameMessage.Type.NetworkUpdate:
                {
                    Sync();
                    break;
                }
                case GameMessage.Type.GroundItemAmountUpdate:
                {
                    UpdateItemAmount(msg.AsGroundItemAmountUpdate());
                    break;
                }

                case GameMessage.Type.EntityEnteredViewRange:
                {
                    var i = GetItem(msg.AsEntityEnteredViewRange());
                    if (i == null) break;
                    NewItem(i);
                    break;
                }
                case GameMessage.Type.EntityLeftViewRange:
                {
                    var i = GetItem(msg.AsEntityLeftViewRange());
                    if (i == null) break;
                    RemoveItem(i);
                    break;
                }

            }
        }
    }
}
