using CScape.Core.Game.Entity;
using CScape.Core.Game.Interface;
using CScape.Core.Game.Item;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Dev.Tests.Internal.Impl
{
    public class MockItem : IItemDefinition
    {
        public bool Equals(IItemDefinition other) => throw new System.NotImplementedException();

        public int ItemId { get; }
        public string Name { get; }
        public int MaxAmount { get; }
        public bool IsTradable { get; }
        public float Weight { get; }
        public bool IsNoted { get; }
        public int NoteSwitchId { get; }

        public MockItem(int itemId, string name, int maxAmount, bool isTradable, float weight, bool isNoted, int noteSwitchId)
        {
            ItemId = itemId;
            Name = name;
            MaxAmount = maxAmount;
            IsTradable = isTradable;
            Weight = weight;
            IsNoted = isNoted;
            NoteSwitchId = noteSwitchId;
        }

        public void UseWith(
            Player player, IContainerInterface ourContainer, 
            int ourIdx, IContainerInterface otherContainer,
            int otherIdx)
        {
            if (_shouldTestAction)
            {
                _shouldTestAction = false;
                Assert.AreEqual(player, _testUsewithPlayer);    
                Assert.AreEqual(ourContainer, _testContainerA);
                Assert.AreEqual(otherContainer, _testContainerB);
                Assert.AreEqual(ourIdx, _testIdxA);
                Assert.AreEqual(otherIdx, _testIdxB);
            }

            WasUseWithCalled = true;
        }

        public void AssertWasUseWithCalled()
        {
            Assert.IsTrue(WasUseWithCalled);
            WasUseWithCalled = false;
        }

        public bool WasUseWithCalled { get; private set; }

        private Player _testUsewithPlayer;
        private IContainerInterface _testContainerA;
        private IContainerInterface _testContainerB;
        private int _testIdxA;
        private int _testIdxB;
        private bool _shouldTestUseWith;

        public void TestForCallToUseWith(
            Player testPlayer, 
            IContainerInterface testContainerA, IContainerInterface testContainerB,
            int testIdxA, int testIdxB)
        {
            Assert.IsFalse(_shouldTestAction);

            _shouldTestAction = true;
            _testUsewithPlayer = testPlayer;
            _testContainerA = testContainerA;
            _testContainerB = testContainerB;
            _testIdxA = testIdxA;
            _testIdxB = testIdxB;
        }


        public void OnAction(
            Player player, IContainerInterface container, 
            int index, ItemActionType type)
        {
            if (_shouldTestAction)
            {
                _shouldTestAction = false;
                Assert.AreEqual(_testActionPlayer, player);
                Assert.AreEqual(_testContainer, container);
                Assert.AreEqual(_testIdx, index);
                Assert.AreEqual(_testType, type);
            }

            WasActionCalled = true;
        }

        public void AssertWasActionCalled()
        {
            Assert.IsTrue(WasActionCalled);
            WasActionCalled = false;
        }

        public bool WasActionCalled { get; private set; }

        private bool _shouldTestAction;
        private Player _testActionPlayer;
        private IContainerInterface _testContainer;
        private int _testIdx;
        private ItemActionType _testType;

        public void TestForCallToAction(
            Player player, IContainerInterface container, 
            int index, ItemActionType type)
        {
            Assert.IsFalse(_shouldTestAction);

            _shouldTestAction = true;
            _testActionPlayer = player;
            _testContainer = container;
            _testIdx = index;
            _testType = type;
        }
    }
}