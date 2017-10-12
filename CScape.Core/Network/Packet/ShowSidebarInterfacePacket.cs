using CScape.Core.Data;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Game.Interface;

namespace CScape.Core.Network.Packet
{
    public sealed class ShowSidebarInterfacePacket : IPacket
    {
        private readonly IGameInterface _interf;
        public const int Id = 71;

        public ShowSidebarInterfacePacket(IGameInterface interf) 
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