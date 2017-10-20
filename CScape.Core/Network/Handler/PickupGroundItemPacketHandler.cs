using System.Linq;
using CScape.Core.Data;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Entities.MovementAction;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;

namespace CScape.Core.Network.Handler
{
    public class PickupGroundItemPacketHandler : IPacketHandler
    {
        public byte[] Handles { get; } = {236};
        public void Handle(Game.Entities.Entity entity, PacketMessage packet)
        {
            var actionComponent = entity.Components.Get<MovementActionComponent>();
            if (actionComponent == null)
                return;

            var y = packet.Data.ReadInt16();
            var id = packet.Data.ReadInt16() + 1;
            var x = packet.Data.ReadInt16();

            // todo : make the player walk to the item they want to pick up before executing pick up code

            var region = entity.GetTransform().PoE.GetRegion(x, y);

            // select first item in the region's item list
            // where the pos of the item matches the packet data
            // and so does the id.
            var query = from h in region.Entities
                where !h.IsDead()
                let ent = h.Get()
                let t = ent.GetTransform()
                where t.X == x && t.Y == y
                let comp = ent.Components.Get<IGroundItemComponent>()
                where comp != null 
                    && comp.Item.Id.ItemId == id
                select comp;

            var item = query.FirstOrDefault();

            if (item == null)
                return;

            if (!entity.CanSee(item.Parent))
                return;

            actionComponent.CurrentAction = new PickupItemAction(entity.Handle, item.Parent.Handle);
        }
    }
}
