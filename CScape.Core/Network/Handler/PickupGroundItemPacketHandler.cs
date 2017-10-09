using System.Linq;
using CScape.Core.Data;
using CScape.Core.Game.Entities.MovementAction;
using CScape.Core.Game.Entity;

namespace CScape.Core.Network.Handler
{
    public class PickupGroundItemPacketHandler : IPacketHandler
    {
        public byte[] Handles { get; } = {236};
        public void Handle(Player player, int opcode, Blob packet)
        {
            var y = packet.ReadInt16();
            var id = packet.ReadInt16() + 1;
            var x = packet.ReadInt16();

            player.Interfaces.OnActionOccurred();

            // todo : make the player walk to the item they want to pick up before executing pick up code
            var region = player.Transform.PoE.GetRegion(x, y);

            // select first item in the region's item list
            // where the pos of the item matches the packet data
            // and so does the id.
            var item = region.Items
                .Where(i => i.Transform.X == x && i.Transform.Y == y)
                .FirstOrDefault(i => i.ItemId == id);

            if (item == null)
            {
                player.Log.Debug(this, $"Pickup item not found: x: {x} y: {y} id: {id}");
                return;
            }

            player.Movement.MoveAction = new PickupItemAction(player, item);
        }
    }
}
