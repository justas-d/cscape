using CScape.Core;
using CScape.Core.Data;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Interface;
using CScape.Core.Injection;
using CScape.Core.Network.Handler;
using CScape.Dev.Tests.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Dev.Tests.Internal.Handler
{
    // todo : test with random blob data

    [TestClass]
    public class ItemOnItemActionPacketHandlerTests
    {
        private (MockServer, Player, ItemOnItemActionPacketHandler, MockItemDb) Data()
        {
            var s = Mock.Server();
            var p = Mock.Player(s);
            var h = new ItemOnItemActionPacketHandler(s.Services);
            var db = s.Services.ThrowOrGet<IItemDefinitionDatabase>() as MockItemDb;
            return (s, p, h, db);
        }

        private void Execute(
            Player p, ItemOnItemActionPacketHandler h,
            int idxA, int idxB,
            int itemIdA, int itemIdB,
            int interfIdA, int interfIdB)
        {
            var b = new Blob(16);
            b.Write16((short)idxB);
            b.Write16((short)idxA);
            b.Write16((short)(itemIdB-1));
            b.Write16((short)interfIdA);
            b.Write16((short)(itemIdA-1));
            b.Write16((short)interfIdB);

            h.HandleAll(p, b);
        }

        private MockMainContainerInterface MainContainerInterf(MockServer s, Player p, int id)
        {
            var interf = new MockMainContainerInterface(s.Services, id);
            Assert.IsTrue(p.Interfaces.TryRegister(interf));
            return interf;
        }

        private MockContainerSidebarInterface SidebarContainerInterf(MockServer s, Player p, int id, int sidebarIdx)
        {
            var interf = new MockContainerSidebarInterface(s, id, sidebarIdx);
            Assert.IsTrue(p.Interfaces.TryRegister(interf));
            return interf;
        }

        private MockMainInterface MainInterfNoContainer(Player p, int id)
        {
            var interf = new MockMainInterface(id);
            Assert.IsTrue(p.Interfaces.TryRegister(interf));
            return interf;
        }

        private MockSidebarInterface SidebarInterfNoContainer(Player p, int id, int sidebarIdx)
        {
            var interf = new MockSidebarInterface(id, sidebarIdx);
            Assert.IsTrue(p.Interfaces.TryRegister(interf));
            return interf;
        }

        

        private void SetupItemForTesting(MockItemDb db, MockItem item,
            Player p,
            IContainerInterface cA, IContainerInterface cB,
            int idxA, int idxB)
        {
            db.PushToQueue(item);
            item.TestForCallToUseWith(p, cA, cB, idxA, idxB);
        }

        private void TestItemOnItemForSuccess(
            MockItemDb db, Player p, ItemOnItemActionPacketHandler h,
            IContainerInterface interfA, IContainerInterface interfB,
            MockItem itemA, int idxA,
            MockItem itemB, int idxB)
        {
            SetupItemForTesting(db, itemA, p, interfA, interfB, idxA, idxB);
            Execute(p, h, idxA, idxB, itemA.ItemId, itemB.ItemId, interfA.Id, interfB.Id);
            itemA.AssertWasUseWithCalled();
        }

        private void TestItemOnItemForFailure(
            MockItemDb db, Player p, ItemOnItemActionPacketHandler h,
            int interfA, int interfB,
            MockItem itemA, int idxA,
            MockItem itemB, int idxB)
        {
            db.PushToQueue(itemA);
            Execute(p, h, idxA, idxB, itemA.ItemId, itemB.ItemId, interfA, interfB);
            Assert.IsFalse(itemA.WasUseWithCalled);
        }

        [TestMethod]
        public void SameInterface()
        {
            var (s, p, h, db) = Data();
            var interf = Mock.Backpack(p);

            var (itemA, _, idxA) = Mock.SetItem(s, interf, 1, 1, 1);
            var (itemB, _, idxB) = Mock.SetItem(s, interf, 5, 5, 5);

            // use b on a
            TestItemOnItemForSuccess(
                db, p, h, interf, interf,
                itemA, idxA, itemB, idxB);

            // use a on b
            TestItemOnItemForSuccess(
                db, p, h, interf, interf,
                itemB, idxB, itemA, idxA);
        }

        [TestMethod]
        public void DifferentInterfaces()
        {
            var (s, p, h, db) = Data();

            var interfA = MainContainerInterf(s, p, 1);
            var interfB = Mock.Backpack(p);

            var (itemA, _, idxA) = Mock.SetItem(s, interfA, 1, 1, 1);
            var (itemB, _, idxB) = Mock.SetItem(s, interfB, 5, 5, 5);

            // use b on a
            TestItemOnItemForSuccess(
                db, p, h, interfA, interfB,
                itemA, idxA, itemB, idxB);

            // use a on b
            TestItemOnItemForSuccess(
                db, p, h, interfB, interfA,
                itemB, idxB, itemA, idxA);
        }

        [TestMethod]
        public void InterfaceAreNotContainers()
        {
            var (s, p, h, db) = Data();
            var dummyInterf = MainContainerInterf(s, p, 1);

            var interfA = Mock.Equipment(p);
            var interfB = Mock.Backpack(p);

            var (itemA, _, idxA) = Mock.SetItem(s, dummyInterf, 1, 1, 1);
            var (itemB, _, idxB) = Mock.SetItem(s, dummyInterf, 5, 5, 5);

            // use b on a
            TestItemOnItemForFailure(
                db, p, h,
                interfA.Id, interfB.Id,
                itemA, idxA, itemB, idxB);

            // use a on b
            TestItemOnItemForFailure(
                db, p, h,
                interfA.Id, interfB.Id,
                itemB, idxB, itemA, idxA);
        }

        [TestMethod]
        public void OneContainerOneNormalInterface()
        {
            var (s, p, h, db) = Data();

            var interfA = Mock.Backpack(p);
            var interfB = Mock.NormalInterface(p);

            var (itemA, _, idxA) = Mock.SetItem(s, interfA, 1, 1, 1);
            var (itemB, _, idxB) = Mock.SetItem(s, interfA, 5, 5, 5);

            // use b on a
            TestItemOnItemForFailure(
                db, p, h,
                interfA.Id, interfB.Id,
                itemA, idxA, itemB, idxB);

            // use a on b
            TestItemOnItemForFailure(
                db, p, h,
                interfA.Id, interfB.Id,
                itemB, idxB, itemA, idxA);
        }

        [TestMethod]
        public void InvalidInterfaceId()
        {
            var (s, p, h, db) = Data();

            var interfA = 123;
            var interfB = 456;
            var idxA = 1;
            var idxB = 2;
            var idA = 1;
            var idB = 1;

            // use b on a
            Execute(p, h, idxA, idxB, idA, idA, interfA, interfB);

            // use a on b
            Execute(p, h, idxB, idxA, idB, idA, interfA, interfB);
        }

        

        [TestMethod]
        public void InvalidIndices()
        {
            var (s, p, h, db) = Data();

            var interfA = MainContainerInterf(s, p, 1);
            var interfB = Mock.Backpack(p);

            var (itemA, _, idxA) = Mock.SetItem(s, interfA, 1, 1, 2);
            var (itemB, _, idxB) = Mock.SetItem(s, interfB, 5, 5, 5);

            idxA *= 2;
            idxB *= 2;

            // use b on a
            TestItemOnItemForFailure(
                db, p, h, interfA.Id, interfB.Id,
                itemA, idxA, itemB, idxB);

            // use a on b
            TestItemOnItemForFailure(
                db, p, h, interfA.Id, interfB.Id,
                itemB, idxB, itemA, idxA);
        }

        [TestMethod]
        public void InvalidItemIds()
        {
            var (s, p, h, db) = Data();

            var interfA = Mock.Equipment(p);
            var interfB = Mock.Backpack(p);

            var (itemA, _, idxA) = Mock.SetItem(s, interfA, 1, 1, 1);
            var (itemB, _, idxB) = Mock.SetItem(s, interfB, 5, 5, 5);

            void TestFail(
                int tInterfA, int tInterfB,
                MockItem tItemA, int tIdxA,
                MockItem tItemB, int tIdxB)
            {
                db.PushToQueue(tItemA);
                Execute(p, h, tIdxA, tIdxB, tItemA.ItemId + 2, tItemB.ItemId + 2, tInterfA, tInterfB);
                Assert.IsFalse(tItemA.WasUseWithCalled);
            }

            // use b on a
            TestFail(
                interfA.Id, interfB.Id,
                itemA, idxA,
                itemB, idxB);

            // use a on b
            TestFail(
                interfA.Id, interfB.Id,
                itemB, idxB,
                itemA, idxA);
        }

        [TestMethod]
        public void UndefinedItem()
        {
            var (s, p, h, db) = Data();
            var interf = MainContainerInterf(s, p, 1);

            var (itemA, _, idxA) = Mock.SetItem(s, interf, MockItemDb.UndefinedId, 1, 1);
            var (itemB, _, idxB) = Mock.SetItem(s, interf, MockItemDb.UndefinedId, 5, 5);

            Execute(p, h, idxA, idxB, MockItemDb.UndefinedId, MockItemDb.UndefinedId, interf.Id, interf.Id);
            Execute(p, h, idxB, idxA, MockItemDb.UndefinedId, MockItemDb.UndefinedId, interf.Id, interf.Id);
        }

        [TestMethod]
        public void UseItemAOnItemA()
        {
            var (s, p, h, db) = Data();
            var interf = MainContainerInterf(s, p, 1);
            var (itemA, _, idxA) = Mock.SetItem(s, interf, 1, 1, 1);

            TestItemOnItemForFailure(
                db, p, h, interf.Id, interf.Id,
                itemA, idxA, itemA, idxA);
        }
    }
}

