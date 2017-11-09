using CScape.Core.Game.Entity.Message;
using CScape.Core.Network.Handler;
using CScape.Core.Tests.Impl;
using CScape.Models.Data;
using CScape.Models.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Core.Tests.Handler
{
    [TestClass]
    public class TalkToPacketHandlerTests
    {
        [TestMethod]
        public void ValidNpc()
        {
            var server = Mock.Server();
            var p = Mock.Player("xd", server).Get();
            var npc = Mock.Npc(server, Mock.Invariant, 1, "testnpc").Get();

            var h = new TalkToPacketHandler(server.Services);
            var b = new Blob(sizeof(short));
            b.Write16(npc.AssertGetNpc().InstanceId);

            h.HandleAll(p, o => PacketMessage.Success((byte)o, b));
        }

        [TestMethod]
        public void NoNpc()
        {
            var server = Mock.Server();
            var p = Mock.Player("xd", server).Get();

            var h = new TalkToPacketHandler(server.Services);
            var b = new Blob(sizeof(short));
            b.Write16(5);

            h.HandleAll(p, o => PacketMessage.Success((byte)o, b));
        }

        [TestMethod]
        public void WrongNpc()
        {
            var server = Mock.Server();
            var p = Mock.Player("xd", server);
            var npc = Mock.Npc(server, Mock.Invariant, 5, "test npc").Get();

            var h = new TalkToPacketHandler(server.Services);
            var b = new Blob(sizeof(short));
            b.Write16(1234);

            h.HandleAll(p.Get(), o => PacketMessage.Success((byte)o, b));
        }
    }
}
