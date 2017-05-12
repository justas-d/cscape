using JetBrains.Annotations;

namespace CScape.Core.Game.Interface
{
    /// <summary>
    /// Safely manages item adding, removing and look up on the items of an underlying item provider.
    /// </summary>
    public interface IItemManager
    {
        /// <summary>
        /// The underlying item provider.
        /// </summary>
        [NotNull] IItemProvider Provider { get; }

        /// <summary>
        /// The maximum capacity.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Attempts to swap items between two indexes in the provider.
        /// </summary>
        /// <returns>True if swap was exeucted, false otherwise.</returns>
        bool Swap(int idx1, int idx2);

        /// <summary>
        /// Calculates the underlying item provider change info based on the given item definition id and the amount of that item.
        /// </summary>
        /// <param name="deltaAmount">The amount of the item, given by its id, we want to change. Positive numbers add items, negative numbers remove items.
        /// 0 produces an invalid change info.</param>
        ItemProviderChangeInfo CalcChangeInfo(int id, int deltaAmount);

        /// <summary>
        /// Changes the underlying item container as describe in the change info, without taking into account the OverflowAmount.
        /// Does nothing on invalid info.
        /// </summary>
        void ExecuteChangeInfo(ItemProviderChangeInfo info);

        /// <summary>
        /// Returns a sum of the amount of items that share the given id.
        /// </summary>
        int Contains(int id);
    }
}