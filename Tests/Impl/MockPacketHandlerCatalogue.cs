using System;
using CScape.Core.Network;
using CScape.Core.Network.Handler;

namespace CScape.Dev.Tests.Impl
{
    public sealed class MockPacketHandlerCatalogue : IPacketHandlerCatalogue
    {
        public static MockPacketHandlerCatalogue Instance { get; }
            = new MockPacketHandlerCatalogue();

        private MockPacketHandlerCatalogue()
        {
            
        }

        public IPacketHandler GetHandler(byte opcode) => throw new NotImplementedException();

    }
}