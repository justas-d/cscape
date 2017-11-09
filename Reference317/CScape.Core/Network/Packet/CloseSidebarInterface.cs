using CScape.Models.Data;

namespace CScape.Core.Network.Packet
{
    public sealed class CloseSidebarInterface : IPacket
    {
        private readonly int _sidebarInterfaceIndex;
        public const int Id = 71;

        public CloseSidebarInterface(int sidebarInterfaceIndex)
        {
            _sidebarInterfaceIndex = sidebarInterfaceIndex;
        }

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(Id);

            stream.Write16(-1);
            stream.Write((byte)_sidebarInterfaceIndex);

            stream.EndPacket();
        }
    }
}