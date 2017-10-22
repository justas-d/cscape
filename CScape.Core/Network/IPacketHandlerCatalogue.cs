using CScape.Core.Network.Handler;

namespace CScape.Core.Network
{
    public interface IPacketHandlerCatalogue
    {
        IPacketHandler GetHandler(byte opcode);
    }
}