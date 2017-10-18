using CScape.Core.Game.Interface;

namespace CScape.Core.Game.Interfaces
{
    public interface ISwappableItemContainer : IItemContainer
    {
        /// <summary>
        /// Swaps the item at idx1 with the item at idx2.
        /// </summary>
        /// <returns>True if swap was successful, false otherwise.</returns>
        bool Swap(int idx1, int idx2);
    }
}