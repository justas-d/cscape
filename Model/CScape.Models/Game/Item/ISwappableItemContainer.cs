namespace CScape.Models.Game.Item
{
    /// <summary>
    /// Defines an item container thjat  a swap operation
    /// </summary>
    public interface ISwappableItemContainer : IItemContainer
    {
        /// <summary>
        /// Swaps the item at idx1 with the item at idx2.
        /// </summary>
        /// <returns>True if swap was successful, false otherwise.</returns>
        bool Swap(int idx1, int idx2);
    }
}