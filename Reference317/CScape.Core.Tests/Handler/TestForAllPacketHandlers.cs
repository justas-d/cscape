using CScape.Core.Network;
using CScape.Core.Tests.Impl;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Core.Tests.Handler
{
    [TestClass]
    public class TestForAllPacketHandlers
    {

        private (MockServer, IEntity, PacketHandlerCatalogue) Data()
        {
            var s = Mock.Server();
            var p = Mock.Player(s).Get();
            var h = new PacketHandlerCatalogue(s.Services);
            return(s, p, h);
        }

        [TestMethod]
        public void SpamTrash()
        {
            var d = Data();
            foreach (var h in d.Item3.All)
                h.SpamTrash(d.Item2);
        }
    }
}
