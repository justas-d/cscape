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
            throw new System.NotImplementedException();
        }

        public void OnAction(
            Player player, IContainerInterface container, 
            int index, ItemActionType type)
        {
            if (_shouldTest)
            {
                _shouldTest = false;
                Assert.AreEqual(_testPlayer, player);
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

        private bool _shouldTest;
        private Player _testPlayer;
        private IContainerInterface _testContainer;
        private int _testIdx;
        private ItemActionType _testType;

        public void TestForCallToAction(
            Player player, IContainerInterface container, 
            int index, ItemActionType type)
        {
            Assert.IsFalse(_shouldTest);

            _shouldTest = true;
            _testPlayer = player;
            _testContainer = container;
            _testIdx = index;
            _testType = type;
        }
    }
}