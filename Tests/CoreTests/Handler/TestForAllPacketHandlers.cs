using System.Collections.Generic;
using CScape.Core.Network.Handler;
using CScape.Dev.Tests.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Dev.Tests.Internal.Handler
{
    [TestClass]
    public class TestForAllPacketHandlers
    {
        private IEnumerable<IPacketHandler> _handlers;

        private IEnumerable<IPacketHandler> AllHandlers()
        {
            var s = Mock.Server();

            if (_handlers == null)
            {
                var dispatch = new PacketDispatch(s.Services);
                _handlers = dispatch.Handlers;
            }
            return _handlers;
        }

        [TestMethod]
        public void SpamTrash()
        {
            foreach (var h in AllHandlers())
                h.SpamTrash();
        }
    }
}
