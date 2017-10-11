using CScape.Core.Game.Entities.Interface;
using CScape.Core.Network.Packet;
using JetBrains.Annotations;

namespace CScape.Core.Game.Interface
{
    public abstract class SingleUserSidebarInterface : SingleUserShowableInterface, ISidebarInterface
    {
        public int SidebarIndex { get; }

        public SingleUserSidebarInterface(int id, int sidebarIdx, [CanBeNull] IButtonHandler buttonHandler = null) : base(id, buttonHandler)
        {
            SidebarIndex = sidebarIdx;
        }

        public override void Show()
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
            Api.Sidebar[SidebarIndex] = null;
        }

        protected override bool CanCloseRightNow() => true;

        protected override void InternalClose()
            => PushUpdate(new CloseSidebarInterface(SidebarIndex));
    }
}