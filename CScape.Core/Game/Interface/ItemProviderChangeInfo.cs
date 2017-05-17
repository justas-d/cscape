using CScape.Core.Game.Item;

namespace CScape.Core.Game.Interface
{
    /// <summary>
    /// Describes how to manipulate an underlying item provider;
    /// </summary>
    public struct ItemProviderChangeInfo
    {
        /// <summary>
        /// Whether this operation can be carried out. (set if input is invalid, empty, null, container is full etc).
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// At which index in the underlying item array the operation must be carried out.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// The new amount of the item.
        /// </summary>
        public int NewAmount { get; }

        /// <summary>
        /// The new id of the item.
        /// </summary>
        public int NewItemDefId { get; }

        /// <summary>
        /// The amount of the item that, due to maximum stack amounts, or because the contains is full, could not have been added to the container.
        /// </summary>
        public long OverflowAmount { get; }

        public static ItemProviderChangeInfo Invalid => new ItemProviderChangeInfo(false);

        private ItemProviderChangeInfo(bool validity)
        {
            IsValid = validity;
            Index = -1;
            NewItemDefId = -1;
            NewAmount = 0;
            OverflowAmount = 0;
        }

        public static ItemProviderChangeInfo Remove(int index) => new ItemProviderChangeInfo(index);

        /// <summary>
        /// Mangaged remove item ctor
        /// </summary>
        private ItemProviderChangeInfo(int index)
        {
            Index = index;
            NewItemDefId = ItemHelper.EmptyId;
            NewAmount = ItemHelper.EmptyAmount;
            OverflowAmount = 0;
            IsValid = true;
        }

        /// <summary>
        /// Change item state ctor  
        /// </summary>
        public ItemProviderChangeInfo(int index, int newAmount, long overflowAmount, int newItemDefId)
        {
            IsValid = true;
            Index = index;
            NewAmount = newAmount;
            OverflowAmount = overflowAmount;
            NewItemDefId = newItemDefId;
        }
    }
}