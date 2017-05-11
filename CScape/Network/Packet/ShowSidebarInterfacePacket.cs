using CScape.Data;
using CScape.Game.Interface;

namespace CScape.Network.Packet
{
    public sealed class ShowSidebarInterfacePacket : IPacket
    {
        private readonly ISidebarInterface _interf;
        public const int Id = 71;

        public ShowSidebarInterfacePacket(ISidebarInterface interf) 
            => _interf = interf;

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(Id);

            stream.Write16((short)_interf.Id);
            stream.Write((byte)_interf.SidebarIndex);

            stream.EndPacket();
        }
    }
}