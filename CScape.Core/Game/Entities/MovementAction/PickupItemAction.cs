using CScape.Core.Game.Entities.Component;

namespace CScape.Core.Game.Entities.MovementAction
{
    public class PickupItemAction : IMovementDoneAction
    {
        private readonly EntityHandle _who;
        private readonly EntityHandle _item;

        public PickupItemAction(EntityHandle who, EntityHandle item)
        {
            _who = who;
            _item = item;
        }

        public void Execute()
        {
            if (_who.IsDead()) return;
            if (_item.IsDead()) return;

            var whoEnt = _who.Get();
            var itemEnt = _item.Get();

            if (!whoEnt.CanSee(itemEnt))
                return;

            var item = itemEnt.Components.Get<IGroundItemComponent>();
            var invComp = whoEnt.Components.Get<IInventoryComponent>();

            if (item == null || invComp == null)
                return;

            // try to pick up the item
            var info = invComp.Inventory.CalcChangeInfo(item.Item);

            // don't allow pickup if inv is full
            if (!info.IsValid || info.OverflowAmount != 0)
            {
                whoEnt.SystemMessage("Your inventory is full.");
                return;
            }

            // add item to inv
            if (invComp.Inventory.ExecuteChangeInfo(info))
                _item.Destroy(); // destroy ground item if we've successfully added the item to inv.
        }
    }
} 