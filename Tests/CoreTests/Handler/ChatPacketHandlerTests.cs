using System;
using System.Linq;
using System.Text;
using CScape.Core.Extensions;
using CScape.Core.Game;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Network.Handler;
using CScape.Dev.Tests.Impl;
using CScape.Models;
using CScape.Models.Data;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Dev.Tests.CoreTests.Handler
{
    [TestClass]
    public sealed class ChatPacketHandlerTests
    {
        private sealed class MessageListener : EntityComponent
        {
            private  string _exStr;
            private  ChatMessage.TextEffect _exEff;
            private  ChatMessage.TextColor _exColor;
            private  bool _exForced;
            private  int _exTitle;
            public bool Received { get; private set; } = false;

            public MessageListener([NotNull] IEntity parent) : base(parent)
            {

            }

            public void SetExpected(string exStr,
                ChatMessage.TextEffect exEff,
                ChatMessage.TextColor exColor,
                bool exForced,
                int exTitle)
            {
                Received = false;
                _exStr = exStr;
                _exEff = exEff;
                _exColor = exColor;
                _exForced = exForced;
                _exTitle = exTitle;
            }

            public override int Priority { get; }

            public override void ReceiveMessage(IGameMessage msg)
            {
                if (msg.EventId == (int) MessageId.ChatMessage)
                {
                    var data = msg.AsChatMessage().Chat;
                    Received = true;
                    Assert.AreEqual(data.Color, _exColor);
                    Assert.AreEqual(data.Effects, _exEff);
                    Assert.AreEqual(data.IsForced, _exForced);
                    Assert.AreEqual(data.Message, _exStr);
                    Assert.AreEqual(data.Title, _exTitle);
                }
            }

            public void AssertReceived()
            {
                Assert.IsTrue(Received);
            }
        }

        private (ChatPacketHandler, IEntity) Data(string msg, ChatMessage.TextEffect eff, ChatMessage.TextColor col, bool forced, int title)
        {
            var d = Data();
            d.Item2.AssertGetPlayer().TitleId = title;
            d.Item2.Components.AssertGet<MessageListener>().SetExpected(msg, eff, col, forced, title);
            return d;
        }


        private (ChatPacketHandler, IEntity) Data()
        {
            var server = Mock.Server();
            var player = Mock.Player("a", server).Get();
            var l = new MessageListener(player);
            player.Components.Add(l);


            var handler = new ChatPacketHandler();
            return (handler, player);
        }

        private void Exec(ChatPacketHandler h, IEntity p, ChatMessage.TextEffect e, ChatMessage.TextColor c, string m)
        {
            var b = new Blob(512);

            b.Write((byte)e);
            b.Write((byte)c);
            b.WriteString(m);

            h.HandleAll(p, o => PacketMessage.Success((byte)o, b));
        }

        private void Validate(IEntity ent)
        {
            var listener = ent.Components.AssertGet<MessageListener>();
            listener.AssertReceived();
        }

        private void Run(ChatPacketHandler h, IEntity ent, ChatMessage.TextEffect e, ChatMessage.TextColor c, string m, bool forced)
        {
            var p = ent.AssertGetPlayer();
            var msg = ent.Components.AssertGet<MessageListener>();
            msg.SetExpected(m, e, c, forced, p.TitleId);
            Exec(h, ent, e, c, m);
            Validate(ent);
        }

        private IEntity TestStringLength(int len)
        {
            var b = new StringBuilder();
            for (var i = 0; i < len; i++)
                b.Append("a");

            var (h, p) = Data(b.ToString(), ChatMessage.TextEffect.None, ChatMessage.TextColor.Yellow, false, 0);
            Exec(h, p, ChatMessage.TextEffect.None, ChatMessage.TextColor.Yellow, b.ToString());
            return p;
        }

        private void TestForSuccess(IEntity p)
        {
            var msg = p.Components.AssertGet<MessageListener>();
            Assert.IsTrue(msg.Received);
        }
        private void TestForFail(IEntity p) => Assert.IsFalse(p.Components.AssertGet<MessageListener>().Received);

        private void TestForFailAll(ChatPacketHandler h, Blob b, IEntity p)
        {
            h.HandleAll(p, a => PacketMessage.Success((byte)a, b), () => TestForFail(p));
        }

        [TestMethod]
        public void TrashSpam()
        {
            var s = Mock.Server();
            var p = Mock.Player(s).Get();
            new ChatPacketHandler().SpamTrash(p);
        }

        [TestMethod]
        public void DoNothingOnInvalidPacketSize()
        {
            var (h, p) = Data();

            for (var i = 0; i < h.MinimumSize; i++)
            {
                var b = new Blob(i);
                TestForFailAll(h, b, p);
            }
        }

        [TestMethod]
        public void MinimumPacketSizePlusOneUnterminatedStringIsValid()
        {
            var (h, p) = Data();
            var b = new Blob(h.MinimumSize + 1);
            TestForFailAll(h, b, p);
        }

        [TestMethod]
        public void DoNothingOnMinimumPacketSize()
        {
            var (h, p) = Data();

            var b = new Blob(h.MinimumSize);
            TestForFailAll(h, b, p);
        }

        [TestMethod]
        public void AllValidCombinations()
        {
            var (h, p) = Data();

            var msg = "Hello world!";
            foreach (var e in Enum.GetValues(typeof(ChatMessage.TextEffect)).Cast<ChatMessage.TextEffect>())
            {
                foreach (var c in Enum.GetValues(typeof(ChatMessage.TextColor)).Cast<ChatMessage.TextColor>())
                    Run(h, p, e, c, msg, false);
            }
        }

        [TestMethod]
        public void StringTooLong()
        {
            var p = TestStringLength(Blob.MaxStringLength + 1);
            TestForFail(p);
        }

        [TestMethod]
        public void StringMaxLength()
        {
            var p = TestStringLength(Blob.MaxStringLength);
            TestForSuccess(p);
        }

        [TestMethod]
        public void UndefinedEnums()
        {
            var e = (ChatMessage.TextEffect) Enum.GetValues(typeof(ChatMessage.TextEffect)).Length + 1;
            var c = (ChatMessage.TextColor) Enum.GetValues(typeof(ChatMessage.TextColor)).Length + 1;
            var msg = "Hello world";

            var (h, p) = Data(msg, ChatMessage.TextEffect.None, ChatMessage.TextColor.Yellow, false, 0);
            Exec(h, p, e, c, msg);

            var cfg = p.Server.Services.ThrowOrGet<IGameServerConfig>();
            Validate(p);
        }
    }
}
