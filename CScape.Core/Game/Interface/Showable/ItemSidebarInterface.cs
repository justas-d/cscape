namespace CScape.Core.Game.Interface.Showable
{
    public class ItemSidebarInterface : SingleUserSidebarInterface, IContainerInterface
    {
        public IItemContainer Items => _items;

        private readonly AbstractSyncedItemManager _items;

        public ItemSidebarInterface(int id, int sidebarIdx, 
            AbstractSyncedItemManager items, IButtonHandler buttonHandler) : 
            base(id, sidebarIdx, buttonHandler)
        {
            _items = items;
        }
    }
}
