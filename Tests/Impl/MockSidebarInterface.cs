using CScape.Core.Game.Interface;
using JetBrains.Annotations;

namespace CScape.Dev.Tests.Impl
{
    public class MockContainerSidebarInterface : MockSidebarInterface, IContainerInterface
    {
        public MockContainerSidebarInterface(
            MockServer server,
            int id, int sidebarIdx, 
            [CanBeNull] IButtonHandler buttonHandler = null) 
            : base(id, sidebarIdx, buttonHandler)
        {
            Items = new BasicItemManager(id, server.Services, new MockItemProvider(28));
        }

        public IItemManager Items { get; }
    }

    public class MockSidebarInterface : SingleUserShowableInterface, ISidebarInterface
    {
        public int SidebarIndex { get; }

        public MockSidebarInterface(int id, int sidebarIdx, [CanBeNull] IButtonHandler buttonHandler = null) : base(id, buttonHandler)
        {
            SidebarIndex = sidebarIdx;
        }

        public override void Show() { }

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

        protected override bool CanCloseRightNow() => true;

        protected override void InternalClose() { }
    }
}