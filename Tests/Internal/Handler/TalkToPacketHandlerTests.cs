using CScape.Core.Data;
using CScape.Core.Network.Handler;
using CScape.Dev.Tests.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Dev.Tests.Internal.Handler
{
    [TestClass]
    public class TalkToPacketHandlerTests
    {
        [TestMethod]
        public void ValidNpc()
        {
            var server = Mock.Server();
            var p = Mock.Player("xd", server);
            var npc = Mock.Npc(server, 5);

            var h = new TalkToPacketHandler();
            var b = new Blob(sizeof(short));
            b.Write16(npc.UniqueNpcId);

            h.HandleAll(p, b);
        }

        [TestMethod]
        public void NoNpc()
        {
            var server = Mock.Server();
            var p = Mock.Player("xd", server);

            var h = new TalkToPacketHandler();
            var b = new Blob(sizeof(short));
            b.Write16(5);

            h.HandleAll(p, b);
        }

        [TestMethod]
        public void WrongNpc()
        {
            var server = Mock.Server();
            var p = Mock.Player("xd", server);
            var npc = Mock.Npc(server, 5);

            var h = new TalkToPacketHandler();
            var b = new Blob(sizeof(short));
            b.Write16(1234);

            h.HandleAll(p, b);
        }
    }
}
