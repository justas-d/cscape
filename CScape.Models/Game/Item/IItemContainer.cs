using System.Collections.Generic;
using JetBrains.Annotations;

namespace CScape.Models.Game.Item
{
    /// <summary>
    /// Defines an item container which can safely manage item adding, removing and look up on the items of an underlying item provider.
    /// </summary>
    public interface IItemContainer
    {
        /// <summary>
        /// The underlying item provider.
        /// </summary>
        [NotNull] IList<ItemStack> Provider { get; }

        /// <summary>
        /// Calculates the underlying item provider change info based on the
        /// given item definition id and the amount of that item.
        /// </summary>
        /// <param name="delta">The item, for which change info will be calculated</param>
        ItemChangeInfo CalcChangeInfo(ItemStack delta);

        /// <summary>
        /// Changes the underlying item container as describe in the change info, without taking into account the OverflowAmount.
        /// Does nothing on invalid info.
        /// </summary>
        /// <returns>True if executed succesfully, false otherwise.</returns>
        bool ExecuteChangeInfo(ItemChangeInfo info);

        /// <summary>
        /// Returns a sum of the amount of items that share the given id.
        /// </summary>
        int Count(int id);
    }
}