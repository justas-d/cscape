using System;
using System.Linq;
using System.Text;
using CScape.Core.Game;
using CScape.Core.Game.Entities.Message;
using CScape.Core.Game.Entity;
using CScape.Core.Network.Handler;
using CScape.Dev.Tests.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Dev.Tests.Internal.Handler
{
    [TestClass]
    public sealed class ChatPacketHandlerTests
    {
        private (ChatPacketHandler, Player) Data()
        {
            var server = Mock.Server();
            var player = Mock.Player("a", server);
            var handler = new ChatPacketHandler();
            return (handler, player);
        }

        private void Exec(ChatPacketHandler h, Player p, ChatMessage.TextEffect e, ChatMessage.TextColor c, string m)
        {
            var b = new Blob(512);

            b.Write((byte)e);
            b.Write((byte)c);
            b.WriteString(m);

            h.HandleAll(p, b);
        }

        private void Validate(Player p, ChatMessage.TextEffect e, ChatMessage.TextColor c, string m)
        {
            Assert.IsNotNull(p.LastChatMessage);
            Assert.IsFalse(p.LastChatMessage.IsForced);
            Assert.AreEqual(e, p.LastChatMessage.Effects);
            Assert.AreEqual(c, p.LastChatMessage.Color);
        }

        private void Run(ChatPacketHandler h, Player p, ChatMessage.TextEffect e, ChatMessage.TextColor c, string m)
        {
            Exec(h, p, e, c, m);
            Validate(p, e,c,m);
        }

        private Player TestStringLength(int len)
        {
            var b = new StringBuilder();
            for (var i = 0; i < len; i++)
                b.Append("a");

            var (h, p) = Data();
            Exec(h, p, ChatMessage.TextEffect.None, ChatMessage.TextColor.Yellow, b.ToString());
            return p;
        }

        private void TestForFail(ChatPacketHandler h, Blob b, Player p)
        {
            foreach (var o in h.Handles)
            {
                h.Handle(p, o, b);
                Assert.IsNull(p.LastChatMessage);
            }
        }

        [TestMethod]
        public void TrashSpam() => new ChatPacketHandler().SpamTrash();

        [TestMethod]
        public void DoNothingOnInvalidPacketSize()
        {
            var (h, p) = Data();

            for (var i = 0; i < h.MinimumSize; i++)
            {
                var b = new Blob(i);
                TestForFail(h, b, p);
            }
        }

        [TestMethod]
        public void MinimumPacketSizePlusOneUnterminatedStringIsValid()
        {
            var (h, p) = Data();
            var b = new Blob(h.MinimumSize + 1);
            TestForFail(h, b, p);
        }

        [TestMethod]
        public void DoNothingOnMinimumPacketSize()
        {
            var (h, p) = Data();

            var b = new Blob(h.MinimumSize);
            TestForFail(h, b, p);
        }

        [TestMethod]
        public void AllValidCombinations()
        {
            var (h, p) = Data();

            var msg = "Hello world!";
            foreach (var e in Enum.GetValues(typeof(ChatMessage.TextEffect)).Cast<ChatMessage.TextEffect>())
            {
                foreach (var c in Enum.GetValues(typeof(ChatMessage.TextColor)).Cast<ChatMessage.TextColor>())
                    Run(h, p, e, c, msg);
            }
        }

        [TestMethod]
        public void StringTooLong()
        {
            var p = TestStringLength(Blob.MaxStringLength + 1);
            Assert.IsNull(p.LastChatMessage);
        }

        [TestMethod]
        public void StringMaxLength()
        {
            var p = TestStringLength(Blob.MaxStringLength);
            Assert.IsNotNull(p.LastChatMessage);
        }

        [TestMethod]
        public void UndefinedEnums()
        {
            var e = (ChatMessage.TextEffect) Enum.GetValues(typeof(ChatMessage.TextEffect)).Length + 1;
            var c = (ChatMessage.TextColor) Enum.GetValues(typeof(ChatMessage.TextColor)).Length + 1;
            var msg = "Hello world";

            var (h, p) = Data();
            Exec(h, p, e, c, msg);

            var cfg = p.Server.Services.ThrowOrGet<IGameServerConfig>();
            Validate(p, cfg.DefaultChatEffect, cfg.DefaultChatColor, msg);
        }

        // undefined enums
    }
}
