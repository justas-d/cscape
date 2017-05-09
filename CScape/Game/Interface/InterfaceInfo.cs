namespace CScape.Game.Interface
{
    public struct InterfaceInfo
    {
        public InterfaceInfo(InterfaceType type, int sidebarIndex)
        {
            Type = type;
            SidebarIndex = sidebarIndex;
        }

        public enum InterfaceType
        {
            Main,
            Input,
            Sidebar
        }

        public InterfaceType Type { get; }
        public int SidebarIndex { get; }
    }
}