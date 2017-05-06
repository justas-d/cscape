namespace CScape.Game.Interface
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
        /// The difference in amount that the operation will apply.
        /// </summary>
        public int AmountDelta { get; }

        public int ItemDefId { get; }
        /// <summary>
        /// The amount of the item that, due to maximum stack amounts, or because the contains is full, could not have been added to the container.
        /// </summary>
        public long OverflowAmount { get; }

        public static ItemProviderChangeInfo Invalid => new ItemProviderChangeInfo(false);

        private ItemProviderChangeInfo(bool validity)
        {
            IsValid = validity;
            Index = -1;
            ItemDefId = -1;
            AmountDelta = 0;
            OverflowAmount = 0;
        }

        public ItemProviderChangeInfo(int index, int amountDelta, long overflowAmount, int itemDefId)
        {
            IsValid = true;
            Index = index;
            AmountDelta = amountDelta;
            OverflowAmount = overflowAmount;
            ItemDefId = itemDefId;
        }
    }
}