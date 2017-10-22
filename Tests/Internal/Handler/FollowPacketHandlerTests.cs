using CScape.Core.Game.Entity;
using CScape.Core.Network.Handler;
using CScape.Dev.Tests.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FollowDirectionProvider = CScape.Core.Game.Entities.Directions.FollowDirectionProvider;

namespace CScape.Dev.Tests.Internal.Handler
{
    [TestClass]
    public class FollowPacketHandlerTests
    {
        private (MockServer, Player, FollowPacketHandler) Data()
        {
            var s = Mock.Server();
            var p = Mock.Player(s);
            var h = new FollowPacketHandler();
            return (s, p, h);
        }

        private void Exec(Player p, FollowPacketHandler h, short target)
        {
            var b = new Blob(16);
            b.Write16(target);
            h.HandleAll(p,b);
        }

        private void Validate(Player us, IWorldEntity target)
        {
            var dir = us.Movement.Directions as FollowDirectionProvider;

            Assert.IsNotNull(dir);
            Assert.AreEqual(dir.Us, us);
            Assert.AreEqual(dir.Target, target);
            Assert.AreEqual(us.InteractingEntity, target);
        }

        [TestMethod]
        public void ValidTarget()
        {
            var (s, p, h) = Data();
            var target = Mock.Player(s);

            Exec(p, h, target.Pid);
            Validate(p, target);
        }

        [TestMethod]
        public void InvalidTarget()
        {
            var (s, p, h) = Data();
            var target = Mock.Player(s);

            Exec(p, h, 654);

            Assert.IsNull(p.Movement.Directions);
            Assert.IsNull(p.InteractingEntity);
        }
    }
}
