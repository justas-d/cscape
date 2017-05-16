namespace CScape.Core.Game.Interface
{
    public interface ISwappableItemManager
    {
        /// <summary>
        /// Attempts to swap items between two indexes in the provider.
        /// </summary>
        /// <returns>True if swap was exeucted, false otherwise.</returns>
        bool Swap(int idx1, int idx2);
    }
}