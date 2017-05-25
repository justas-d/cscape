namespace CScape.Core.Game.Entity
{
    public class PickupItemAction : IMovementDoneAction
    {
        private readonly Player _player;
        private readonly GroundItem _item;

        public PickupItemAction(Player player, GroundItem item)
        {
            _player = player;
            _item = item;
        }

        public void Execute()
        {
            // try to pick up the item
            var info = _player.Inventory.CalcChangeInfo(_item.ItemId, _item.ItemAmount);

            // don't allow pickup if inv is full
            if (!info.IsValid || info.OverflowAmount != 0)
            {
                _player.SendSystemChatMessage("Your inventory is full.");
                return;
            }

            // add item to inv
            if (_player.Inventory.ExecuteChangeInfo(info))
                _item.Destroy(); // destroy ground item if we've successfully added the item to inv.
        }
    }
}