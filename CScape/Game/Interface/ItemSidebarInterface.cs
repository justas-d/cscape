using CScape.Network.Packet;

namespace CScape.Game.Interface
{
    public class ItemSidebarInterface : SingleUserApiInterface, IShowableInterface, IContainerInterface, ISidebarInterface
    {
        public int SidebarIndex { get; }
        public IButtonHandler ButtonHandler { get; }
        public IItemManager Items => _items;

        private readonly InterfacedItemManager _items;

        public ItemSidebarInterface(int id, int sidebarIdx, InterfacedItemManager items, IButtonHandler buttonHandler) : base(id)
        {
            SidebarIndex = sidebarIdx;
            _items = items;
            ButtonHandler = buttonHandler;
        }

        public void Show()
        {
            PushUpdate(new ShowSidebarInterfacePacket(this));
        }

        protected override bool InternalRegister(IInterfaceManagerApiBackend api)
        {
            if (!api.Frontend.CanShow(InterfaceType.Sidebar, SidebarIndex))
                return false;

            api.Sidebar[SidebarIndex] = this;
            return true;
        }

        protected override void InternalUnregister()
        {
            if (Api == null) return;
            Api.Sidebar[SidebarIndex] = this;
        }

        public bool TryClose()
        {
            if (Api == null)
                return false;

            PushUpdate(new CloseSidebarInterface(SidebarIndex));

            Api.NotifyOfClose(this);
            return true;
        }
    }
}
