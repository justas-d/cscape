using System;
using System.Linq;
using CScape.Core.Data;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Interface;
using CScape.Core.Network.Handler;
using CScape.Dev.Tests.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Dev.Tests.Internal.Handler
{
    [TestClass]
    public class SwapItemPacketHandlerTests
    {
        private (MockServer, Player, SwapItemPacketHandler) Data()
        {
            var s = Mock.Server();
            var p = Mock.Player(s);
            var h = new SwapItemPacketHandler();
            return(s, p, h);
        }

        private void Execute(
            SwapItemPacketHandler h, Player p,
            int idxA, int idxB,
            int interfaceId)

        {
            var b = new Blob(7);
            b.Write16((short) interfaceId);
            b.Write(0); //magic
            b.Write16((short) idxA);
            b.Write16((short) idxB);

            h.HandleAll(p, b);
        }

        private void TestSwapped(
            IItemProvider prov,
            int idxA, int idA,
            int idxB, int idB)
        {
            Assert.AreEqual(prov.GetId(idxA), idB);
            Assert.AreEqual(prov.GetId(idxB), idA);
        }

        private void TestNothingHappened(
            IItemProvider prov,
            int idxA, int idA,
            int idxB, int idB)
        {
            Assert.AreEqual(prov.GetId(idxA), idA);
            Assert.AreEqual(prov.GetId(idxB), idB);
        }

        private void TestSuccess(
            IItemProvider prov,
            int idxA, int idA,
            int idxB, int idB)
        {
            TestSwapped(prov, idxA, idA, idxB, idB);
        }

        private void TestFailure(
            IItemProvider prov,
            int idxA, int idA,
            int idxB, int idB)
        {
            TestNothingHappened(prov, idxA, idA, idxB, idB);
        }


        private void DoubleTest(
            SwapItemPacketHandler h, Player p,
            int idxA, int idxB,
            int idA, int idB,
            IContainerInterface interf,
            Action<IItemProvider, int, int, int, int> first,
            Action<IItemProvider, int, int, int, int> second)
        {
            Execute(h, p, idxA, idxB, interf.Id);
            first(interf.Items.Provider, idxA, idA, idxB, idB);

            Execute(h, p, idxA, idxB, interf.Id);
            second(interf.Items.Provider, idxA, idA, idxB, idB);
        }

        [TestMethod]
        public void ValidSwap()
        {
            var (s, p, h) = Data();
            var interf = Mock.Backpack(p);

            var (itemA, _, idxA) = Mock.SetItem(s, interf, 1, 1, 1);
            var (itemB, _, idxB) = Mock.SetItem(s, interf, 2, 2, 2);

            DoubleTest(h, p, 
                idxA, idxB, 
                itemA.ItemId, itemB.ItemId, 
                interf,
                TestSwapped,
                TestNothingHappened);
        }

        [TestMethod]
        public void EqualIndicesYieldNoSwap()
        {
            var (s, p, h) = Data();
            var interf = Mock.Backpack(p);
            var (itemA, _, idxA) = Mock.SetItem(s, interf, 1, 1, 1);

            Execute(h, p, idxA, idxA, interf.Id);
            TestFailure(interf.Items.Provider, idxA, itemA.ItemId, idxA, itemA.ItemId);
        }

        [TestMethod]
        public void InvalidContainerYieldsNoSwap()
        {
            var invalidId = 0;

            var (s, p, h) = Data();
            var interf = Mock.Backpack(p);
            var (itemA, _, idxA) = Mock.SetItem(s, interf, 1, 1, 1);
            var (itemB, _, idxB) = Mock.SetItem(s, interf, 2, 2, 2);

            Execute(h, p, idxA, idxB, invalidId);
            TestFailure(interf.Items.Provider, idxA, itemA.ItemId, idxB, itemB.ItemId);
        }

        [TestMethod]
        public void WhenContainerIsNormalInterfaceYieldNoSwap()
        {
            var (s, p, h) = Data();
            var invalidInterf = Mock.NormalInterface(p);

            var interf = Mock.Backpack(p);
            var (itemA, _, idxA) = Mock.SetItem(s, interf, 1, 1, 1);
            var (itemB, _, idxB) = Mock.SetItem(s, interf, 2, 2, 2);

            Execute(h, p, idxA, idxB, invalidInterf.Id);
            TestFailure(interf.Items.Provider, idxA, itemA.ItemId, idxB, itemB.ItemId);
        }

        private void TestOutOfRangeIndices(int overrideIdxA, int overrideIdxB)
        {
            var (s, p, h) = Data();
            var interf = Mock.Backpack(p);
            var (itemA, _, idxA) = Mock.SetItem(s, interf, 1, 1, 1);
            var (itemB, _, idxB) = Mock.SetItem(s, interf, 2, 2, 2);

            // take note of provider state
            var itemsBefore = interf.Items.Provider.ToList();

            Execute(h, p, overrideIdxA, overrideIdxB, interf.Id);

            // verify state of provider didn't change
            var prov = interf.Items.Provider;
            for (int i = 0; i < prov.Count; i++)
            {
                Assert.AreEqual(itemsBefore[i].id, prov[i].id);
                Assert.AreEqual(itemsBefore[i].amount, prov[i].amount);
            }
        }

        [TestMethod]
        public void OutOfRangeIndicesYieldNoSwap()
        {
            TestOutOfRangeIndices(500, 501);
        }

        [TestMethod]
        public void OneValidIndexAndOneInvalidIndexYieldsNoSwap()
        {
            TestOutOfRangeIndices(1, 501);
            TestOutOfRangeIndices(500, 2);
        }
    }
}
