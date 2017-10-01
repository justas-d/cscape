using CScape.Core.Network.Handler;

namespace CScape.Core.Injection
{
    public interface IPacketHandlerCatalogue
    {
        IPacketHandler GetHandler(byte opcode);
    }
}