using JetBrains.Annotations;

namespace CScape.Game.Interface
{
    /// <summary>
    /// Safely manages item adding, removing and look up on the items of an underlying item provider.
    /// </summary>
    public interface IInterfaceItemManager
    {
        /// <summary>
        /// The interface id of the item container on the client.
        /// </summary>
        int ContainerInterfaceId { get; }

        /// <summary>
        /// The amount of items that aren't empty.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// The underlying item provider.
        /// </summary>
        [NotNull] IItemProvider Provider { get; }
        /// <summary>
        /// The maximum capacity.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Calculates the underlying item provider change info based on the given item definition id and the amount of that item.
        /// </summary>
        /// <param name="amount">The amount of the item, given by its id, we want to change. Positive numbers add items, negative numbers remove items.
        /// 0 produces an invalid change info.</param>
        ItemProviderChangeInfo CalcChangeInfo(int id, int amount);

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