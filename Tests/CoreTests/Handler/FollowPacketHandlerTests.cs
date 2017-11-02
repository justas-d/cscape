using CScape.Core.Game.Entity;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Game.Entity.Directions;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Network.Handler;
using CScape.Dev.Tests.Impl;
using CScape.Models.Data;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Dev.Tests.Internal.Handler
{
    [TestClass]
    public class FollowPacketHandlerTests
    {
        private (MockServer, IEntity, FollowPacketHandler) Data()
        {
            var s = Mock.Server();
            var p = Mock.Player(s).Get();
            var h = new FollowPacketHandler(s.Services);
            p.Components.Add(new TileMovementComponent(p));
            return (s, p, h);
        }

        private void Exec(IEntity p, FollowPacketHandler h, short target)
        {
            var b = new Blob(16);
            b.Write16(target);
            h.HandleAll(p, o => PacketMessage.Success((byte)o ,b));
        }

        private void Validate(IEntity us, IEntity target)
        {
            var dir = us.Components.AssertGet<TileMovementComponent>().Directions as FollowDirectionProvider;

            Assert.IsNotNull(dir);
            Assert.AreEqual(dir.Target, target.Handle);
            Assert.AreEqual(us.GetTransform().InteractingEntity.Entity.Get(), target);
        }

        [TestMethod]
        public void ValidTarget()
        {
            var (s, p, h) = Data();
            var target = Mock.Player("follow target", s).Get();

            Exec(p, h, (short)target.AssertGetPlayer().InstanceId);
            Validate(p, target);
        }

        // TODO : test follow self

        public void ExecInvalidTarget((MockServer s, IEntity p, FollowPacketHandler h) data, short id)
        {
            Exec(data.p, data.h, id);

            Assert.IsNull(data.p.Components.AssertGet<TileMovementComponent>().Directions);
            Assert.IsNull(data.p.GetTransform().InteractingEntity.Entity);
        }

        [TestMethod]
        public void InvalidTarget()
        {
            var d = Data();
            var randomPlayer = Mock.Player("random player", d.Item1);

            ExecInvalidTarget(d, 564);
        }

        [TestMethod]
        public void FollowSelf()
        {
            var d = Data();

            ExecInvalidTarget(d, (short)d.Item2.AssertGetPlayer().InstanceId);
        }
    }
}
