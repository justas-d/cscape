using System;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Interface;
using JetBrains.Annotations;

namespace CScape.Models.Game.Item
{
    public class ItemStack : IEquatable<ItemStack>
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
            public void UseWith(IEntity entity, ItemStack other)
            { 
            }

            public void OnAction(IEntity parentEntity, IItemContainer itemsContainer, int itemIndexInContainer,
                InterfaceMetadata containerInterfaceMetadata, ItemStack item, int actionId)
            {
                
            }
        }

        public static IItemDefinition EmptyItem { get; } = new EmptyItemDefinition();
        public static ItemStack Empty { get; } = new ItemStack(EmptyItem, EmptyAmount);

        public const int EmptyAmount = 0;

        [NotNull]
        public IItemDefinition Id { get; }
        public int Amount { get; }

        /// <summary>
        /// Constructs an empty item stack.
        /// </summary>
        public ItemStack()
        {
            Id = EmptyItem;
            Amount = 0;
        }

        /// <summary>
        /// Constructs an item stack.
        /// </summary>
        public ItemStack([NotNull] IItemDefinition id, int amount)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Amount = amount;
        }

        public bool IsFull() => Amount >= Id.MaxAmount;
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
