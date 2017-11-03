using CScape.Core.Game.Entity.Message;
using CScape.Core.Network.Handler;
using CScape.Dev.Tests.Impl;
using CScape.Models.Data;
using CScape.Models.Extensions;
using CScape.Models.Game.Command;
using CScape.Models.Game.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Dev.Tests.CoreTests.Handler
{
    [TestClass]
    public class CommandPacketHandlerTests
    {
        public class MockCommandHandler : ICommandHandler
        {
            private readonly string _expectedCmd;

            public bool WasCalled { get; private set; }

            public MockCommandHandler(string expectedCmd)
            {
                _expectedCmd = expectedCmd;
            }

            public bool Push(IEntity callee, string input)
            {
                WasCalled = true;
                Assert.AreEqual(input, _expectedCmd);
                return true;
            }
        }

        private (IEntity, CommandPacketHandler, MockCommandHandler) Data(string targetStr)
        {
            var c = new ServiceCollection();
            var ch = new MockCommandHandler(targetStr);
            c.AddSingleton<ICommandHandler>(_ => ch);
            var s = Mock.Server(c);

            var p = Mock.Player("a", s).Get();
            return (p, new CommandPacketHandler(s.Services), ch);
        }

        private void TestFail(
            CommandPacketHandler h, MockCommandHandler ch, IEntity p, Blob b)
        {
            h.HandleAll(p, o => PacketMessage.Success((byte)o, b), () => Assert.IsFalse(ch.WasCalled));
        }

        private void TestSuccess(
            CommandPacketHandler h, MockCommandHandler ch, IEntity p, Blob b)
        {
            h.HandleAll(p, o => PacketMessage.Success((byte)o, b), () => Assert.IsTrue(ch.WasCalled));
        }

        [TestMethod]
        public void ValidCommand()
        {
            var input = "hello world     aaa asff sg45646c zxzxc";
            var (p, h, ch) = Data(input);

            var b = new Blob(256);
            b.WriteString(input);

            TestSuccess(h, ch, p, b);
        }

        [TestMethod]
        public void EmptyBlob()
        {
            var (p, h, ch) = Data(null);
            var b = new Blob(0);
            TestFail(h, ch, p, b);
        }

        [TestMethod]
        public void CommandNotNullTerminated()
        {
            var (p, h, ch) = Data(null);
            var b = new Blob(5);
            TestFail(h, ch, p, b);
        }
    }
}
