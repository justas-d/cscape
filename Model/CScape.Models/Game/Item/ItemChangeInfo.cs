namespace CScape.Models.Game.Item
{
    /// <summary>
    /// Describes how to manipulate an underlying item provider;
    /// </summary>
    public struct ItemChangeInfo
    {
        /// <summary>
        /// Whether this operation can be carried out. (set if input is invalid, empty, null, container is full etc).
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// At which index in the underlying item array the operation must be carried out.
        /// </summary>
        public int Index { get; }

        public ItemStack NewItem { get; }

        /// <summary>
        /// The amount of the item that, due to maximum stack amounts, or because the contains is full, could not have been added to the container.
        /// </summary>
        public long OverflowAmount { get; }

        public static ItemChangeInfo Invalid => new ItemChangeInfo(false);

        private ItemChangeInfo(bool validity)
        {
            IsValid = validity;
            Index = -1;
            NewItem = ItemStack.Empty;
            OverflowAmount = 0;
        }

        public static ItemChangeInfo Remove(int index) => new ItemChangeInfo(index);

        /// <summary>
        /// Mangaged remove item ctor
        /// </summary>
        private ItemChangeInfo(int index)
        {
            Index = index;
            NewItem = ItemStack.Empty;
            OverflowAmount = 0;
            IsValid = true;
        }

        /// <summary>
        /// Change item state ctor  
        /// </summary>
        /// public ItemProviderChangeInfo(int index, int newAmount, long overflowAmount, int newItemDefId)
        public ItemChangeInfo(int index, ItemStack newItem, long overflowAmount)
        {
            IsValid = true;
            Index = index;
            NewItem = newItem;
            OverflowAmount = overflowAmount;
        }
    }
}