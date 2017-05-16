namespace CScape.Core.Game.Interface
{
    public class ItemSidebarInterface : SingleUserSidebarInterface, IContainerInterface
    {
        public IItemManager Items => _items;

        private readonly AbstractSyncedItemManager _items;

        public ItemSidebarInterface(int id, int sidebarIdx, 
            AbstractSyncedItemManager items, IButtonHandler buttonHandler) : 
            base(id, sidebarIdx, buttonHandler)
        {
            _items = items;
        }
    }
}
