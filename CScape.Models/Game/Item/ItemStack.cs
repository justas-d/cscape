using System;

namespace CScape.Models.Game.Item
{
    public struct ItemStack : IEquatable<ItemStack>
    {
        private sealed class EmptyItemDefinition : IItemDefinition
        {
            public bool Equals(IItemDefinition other) => other.ItemId == ItemId;

            public int ItemId => 0;
            public string Name => "null";
            public int MaxAmount => 1;
            public bool IsTradable => false;
            public float Weight => 0;
            public bool IsNoted => false;
            public int NoteSwitchId => 0;

            public void UseWith(Player player, IContainerInterface ourContainers, int ourIdx, IContainerInterface otherContainer,
                int otherIdx)
            {
                throw new NotImplementedException();
            }

            public void OnAction(Player player, IContainerInterface container, int index, ItemActionType type)
            {
                throw new NotImplementedException();
            }
        }

        public static IItemDefinition EmptyItem { get; } = new EmptyItemDefinition();

        public const int EmptyAmount = 0;

        public IItemDefinition Id { get; }
        public int Amount { get; }

        public ItemStack(IItemDefinition id, int amount)
        {
            Id = id;
            Amount = amount;
        }

        public static ItemStack Empty { get; } = new ItemStack(EmptyItem, EmptyAmount);

        public bool IsEmpty() => Equals(Empty) || Amount == EmptyAmount;
        
        public bool Equals(ItemStack other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ItemStack && Equals((ItemStack) obj);
        }

        public override int GetHashCode()
        {
            return Id.ItemId * 13;
        }
    }
}
