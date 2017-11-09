using System.Collections.Generic;
using CScape.Core.Network.Handler;

namespace CScape.Core.Network
{
    public interface IPacketHandlerCatalogue
    {
        IEnumerable<IPacketHandler> All { get; }
        IPacketHandler GetHandler(byte opcode);
    }
}