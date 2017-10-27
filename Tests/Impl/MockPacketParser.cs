using System;
using System.Collections.Generic;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Network;
using CScape.Models.Data;

namespace CScape.Dev.Tests.Impl
{
    public sealed class MockPacketParser : IPacketParser
    {
        public static MockPacketParser Instance { get; } = new MockPacketParser();

        private MockPacketParser()
        {
            
        }

        public IEnumerable<PacketMessage> Parse(CircularBlob stream) => throw new NotImplementedException();
    }
}