using CScape.Game.Entity;

namespace CScape.Game.Interface
{
    public class PlayerItems
    {
        public IItemManager Inventory { get; }
        public IItemManager Bank { get; }
    }

    public class ItemSidebarInterface : AbstractManagedInterface, IItemInterface
    {
        public Player Player { get; }
        public IItemManager Items { get; }

        public ItemSidebarInterface(Player player, IItemManager items, int interfaceId, int sidebarId) : base(interfaceId, new InterfaceInfo(InterfaceInfo.InterfaceType.Sidebar, sidebarId))
        {
            Player = player;
            Items = items;
        }

        protected override bool InternalTryClose() => true;

    }
}
