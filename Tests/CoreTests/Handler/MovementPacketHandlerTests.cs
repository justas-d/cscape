using CScape.Core.Game.Entity;
using CScape.Core.Game.Entity.Directions;
using CScape.Core.Network.Handler;
using CScape.Dev.Tests.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Dev.Tests.Internal.Handler
{
    [TestClass]
    public class MovementPacketHandlerTests
    {
        private void TestSuccess(Player p)
        {
            Assert.IsTrue(p.Movement.Directions is ByReferenceWithDeltaWaypointsDirectionsProvider);   
        }

        private void TestFailure(Player p)
        {
            Assert.IsNull(p.Movement.Directions);
        }

        private (MockServer, Player, MovementPacketHandler) Data()
        {
            var s = Mock.Server();
            var p = Mock.Player(s);
            var h = new MovementPacketHandler();
            return (s, p, h);
        }

        [TestMethod]
        public void NoWaypointsYieldNoProvider()
        {
            var (_, p, h) = Data();
            h.HandleAll(p, new Blob(0), () => TestFailure(p));
        }

        [TestMethod]
        public void MaxWaypointYieldsProvider()
        {
            var (_, p, h) = Data();
            h.HandleAll(p, new Blob((h.MaxTiles * 2) + 1), () => TestSuccess(p));
        }

        [TestMethod]
        public void MoreThenMaxWaypointYieldsNoProvider()
        {
            var (_, p, h) = Data();
            h.HandleAll(p, new Blob((h.MaxTiles * 2) + 2), () => TestFailure(p));
        }

        [TestMethod]
        public void UnevenDeltaWaypointArrayYieldsNoProvider()
        {
            var (_, p, h) = Data();
            h.HandleAll(p, new Blob(h.MaxTiles * 2), () => TestFailure(p));
        }
    }
}
