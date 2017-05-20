﻿using CScape.Core.Data;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using CScape.Core.Network.Handler;
using CScape.Dev.Tests.Internal.Impl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Dev.Tests.Internal
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

        private (Player, CommandPacketHandler) Data(MockCommandHandler ch)
        {
            var c = new ServiceCollection();
            c.AddSingleton<ICommandHandler>(_ => ch);
            var s = Mock.Server(c);

            var p = Mock.Player("a", s);
            return (p, new CommandPacketHandler(s.Services));
        }

        [TestMethod]
        public void ValidCommand()
        {
            var input = "hello world     aaa asff sg45646c zxzxc";
            var ch = new MockCommandHandler(input);
            var (p, h) = Data(ch);

            var b = new Blob(256);
            b.WriteString(input);

            h.Handle(p, h.Handles[0], b);
            Assert.IsTrue(ch.WasCalled);
        }
    }
}