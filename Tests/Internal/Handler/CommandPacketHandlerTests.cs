using CScape.Core.Game.Entity;
using CScape.Core.Network.Handler;
using CScape.Dev.Tests.Impl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Dev.Tests.Internal.Handler
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

            public bool Push(Player callee, string input)
            {
                WasCalled = true;
                Assert.AreEqual(input, _expectedCmd);
                return true;
            }
        }

        private (Player, CommandPacketHandler, MockCommandHandler) Data(string targetStr)
        {
            var c = new ServiceCollection();
            var ch = new MockCommandHandler(targetStr);
            c.AddSingleton<ICommandHandler>(_ => ch);
            var s = Mock.Server(c);

            var p = Mock.Player("a", s);
            return (p, new CommandPacketHandler(s.Services), ch);
        }

        private void TestFail(
            CommandPacketHandler h, MockCommandHandler ch, Player p, Blob b)
        {
            h.HandleAll(p, b, () => Assert.IsFalse(ch.WasCalled));
        }

        private void TestSuccess(
            CommandPacketHandler h, MockCommandHandler ch, Player p, Blob b)
        {
            h.HandleAll(p, b, () => Assert.IsTrue(ch.WasCalled));
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
