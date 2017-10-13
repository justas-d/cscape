using CScape.Core.Data;
using CScape.Core.Game.Entities.Interface;

namespace CScape.Core.Network.Packet
{
    public sealed class ShowSidebarInterfacePacket : IPacket
    {
        private readonly IGameInterface _interf;
        private readonly byte _sidebarIndex;
        public const int Id = 71;

        public ShowSidebarInterfacePacket(IGameInterface interf, byte sidebarIndex)
        {
            _interf = interf;
            _sidebarIndex = sidebarIndex;
        }

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(Id);

            stream.Write16((short)_interf.Id);
            stream.Write(_sidebarIndex);

            stream.EndPacket();
        }
    }
}