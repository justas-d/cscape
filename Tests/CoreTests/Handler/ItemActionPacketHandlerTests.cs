using System.Linq;
using CScape.Core;
using CScape.Core.Game.Entity;
using CScape.Core.Network.Handler;
using CScape.Dev.Tests.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Dev.Tests.Internal.Handler
{
    [TestClass]
    public class ItemActionPacketHandlerTests
    {
        private (MockItemDb db, MockItem i) ItemData(MockServer s, int id)
        {
            var def = s.Services.ThrowOrGet<IItemDefinitionDatabase>() as MockItemDb;
            return (def, def.Get(id) as MockItem);
        }

        private (MockServer s, Player p, ItemActionPacketHandlers h) Data()
        {
            var s = Mock.Server();
            var p = Mock.Player(s);
            var h = new ItemActionPacketHandlers(s.Services);
            return (s, p, h);
        }

        private void Execute(Player p, ItemActionPacketHandlers h, int opcode,
            short interf, short idx, short id)
        {
            var b = new Blob(16);
            b.Write16(interf);
            b.Write16(idx);
            b.Write16((short) (id - 1));

            h.Handle(p, opcode, b);
        }

        /// <summary>
        /// Validates that the Handles array opcodes equal to the array of keys in OpcodeToActionMap
        /// </summary>
        [TestMethod]
        public void ValidateHandlersMatchActionMap()
        {
            var s = Mock.Server();
            var h = new ItemActionPacketHandlers(s.Services);

            foreach (var opcode in h.Handles)
                Assert.IsTrue(h.OpcodeToActionMap.Keys.Contains(opcode));
        }

        [TestMethod]
        public void OutOfRangeIndicies()
        {
            var (s, p, h) = Data();
            var (db, item) = ItemData(s, 1);
            var interf = new MockMainContainerInterface(s.Services, 1);
            var items = interf.Items.Provider;

            Assert.IsTrue(p.Interfaces.TryShow(interf));

            short idx = 5;
            short amnt = 42;
            items.SetAmount(idx, amnt);
            items.SetId(idx, item.ItemId);

            void TestIndex(short i)
            {
                foreach (var opcode in h.Handles)
                {
                    db.PushToQueue(item);
                    Execute(p, h, opcode, (short)interf.Id, i, (short)item.ItemId);
                    Assert.IsFalse(item.WasActionCalled);
                }
            }

            TestIndex(-1);
            TestIndex((short) (items.Count + 1));
        }

        [TestMethod]
        public void ValidAction()
        {
            var (s, p, h) = Data();
            var (db, item) = ItemData(s, 1);
            var interf = new MockMainContainerInterface(s.Services, 1);
            var items = interf.Items.Provider;

            Assert.IsTrue(p.Interfaces.TryShow(interf));

            short idx = 5;
            short amnt = 42;
            items.SetAmount(idx, amnt);
            items.SetId(idx, item.ItemId);

            foreach (var opcode in h.Handles)
            {
                // setup tests in itemdef
                item.TestForCallToAction(p, interf, idx, h.OpcodeToActionMap[opcode]);
                // make the item db return our itemdef instead of a new one
                db.PushToQueue(item);
                Execute(p, h, opcode, (short)interf.Id, idx, (short)item.ItemId);
                item.AssertWasActionCalled();
            }
        }

        [TestMethod]
        public void InvalidOpcode()
        {
            var (s, p, h) = Data();
            var (db, item) = ItemData(s, 1);
            db.PushToQueue(item);

            Execute(p, h, -1, 0, 0, 0);
            Assert.IsFalse(item.WasActionCalled);
        }

        [TestMethod]
        public void NoInterface()
        {
            var (s, p, h) = Data();
            var (db, item) = ItemData(s, 1);

            short interfId = 654;
            short idx = 5;
            short amnt = 42;

            foreach (var opcode in h.Handles)
            {
                db.PushToQueue(item);
                Execute(p, h, opcode, interfId, idx, (short) item.ItemId);
                Assert.IsFalse(item.WasActionCalled);
            }

        }

        [TestMethod]
        public void InterfaceIsNotAContainer()
        {
            var (s, p, h) = Data();
            var (db, item) = ItemData(s, 1);

            var interf = new MockMainInterface(1);
            Assert.IsTrue(p.Interfaces.TryShow(interf));

            short idx = 5;
            short amnt = 42;

            foreach (var opcode in h.Handles)
            {
                db.PushToQueue(item);
                Execute(p, h, opcode, (short) interf.Id, idx, (short) item.ItemId);
                Assert.IsFalse(item.WasActionCalled);
            }
        }

        [TestMethod]
        public void ItemIdMismatch()
        {
            var (s, p, h) = Data();
            var (db, item) = ItemData(s, 1);
            var interf = new MockMainContainerInterface(s.Services, 1);
            var items = interf.Items.Provider;

            Assert.IsTrue(p.Interfaces.TryShow(interf));

            short idx = 5;
            short amnt = 42;
            items.SetAmount(idx, amnt);
            items.SetId(idx, item.ItemId);

            foreach (var opcode in h.Handles)
            {
                db.PushToQueue(item);
                Execute(p, h, opcode, (short) interf.Id, idx, (short) (item.ItemId * 2));
                Assert.IsFalse(item.WasActionCalled);
            }
        }

        [TestMethod]
        public void UndefinedItem()
        {
            var (s, p, h) = Data();
            var (db, item) = ItemData(s, MockItemDb.UndefinedId);
            var interf = new MockMainContainerInterface(s.Services, 1);
            var items = interf.Items.Provider;

            Assert.IsTrue(p.Interfaces.TryShow(interf));

            short idx = 5;
            short amnt = 42;
            items.SetAmount(idx, amnt);
            items.SetId(idx, MockItemDb.UndefinedId);

            foreach (var opcode in h.Handles)
            {
                db.PushToQueue(item);
                Execute(p, h, opcode, (short)interf.Id, idx, MockItemDb.UndefinedId);
            }
        }
    }
}

