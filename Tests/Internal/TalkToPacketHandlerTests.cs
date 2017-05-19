using CScape.Core.Data;
using CScape.Core.Network.Handler;
using CScape.Dev.Tests.Internal.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Dev.Tests.Internal
{
    [TestClass]
    public class TalkToPacketHandlerTests
    {
        [TestMethod]
        public void ValidNpc()
        {
            var server = Mock.Server();
            var player = Mock.Player("xd", server);
            var npc = Mock.Npc(server, 5);

            var handler = new TalkToPacketHandler();
            var data = new Blob(sizeof(short));
            data.Write16(npc.UniqueNpcId);

            handler.Handle(player, handler.Handles[0], data);
        }

        [TestMethod]
        public void NoNpc()
        {
            var server = Mock.Server();
            var player = Mock.Player("xd", server);

            var handler = new TalkToPacketHandler();
            var data = new Blob(sizeof(short));
            data.Write16(5);

            handler.Handle(player, handler.Handles[0], data);
        }

        [TestMethod]
        public void WrongNpc()
        {
            var server = Mock.Server();
            var player = Mock.Player("xd", server);
            var npc = Mock.Npc(server, 5);

            var handler = new TalkToPacketHandler();
            var data = new Blob(sizeof(short));
            data.Write16(1234);

            handler.Handle(player, handler.Handles[0], data);
        }
    }
}
