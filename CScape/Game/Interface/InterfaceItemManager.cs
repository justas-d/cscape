using System;
using JetBrains.Annotations;

namespace CScape.Game.Interface
{
    public class InterfaceItemManager : IInterfaceItemManager
    {
        /// <summary>
        /// The maximum capacity.
        /// </summary>
        public int Size => Provider.Items.Length;

        /// <summary>
        /// The amount of items that aren't empty.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// The underlying item provider.
        /// </summary>
        [NotNull] public IItemProvider Provider { get; }

        public InterfaceItemManager([NotNull] IItemProvider provider)
        {
            Provider = provider;
        }

        /// <summary>
        /// Calculates the underlying item provider change info based on the given item definition id and the amount of that item.
        /// </summary>
        /// <param name="amount">The amount of the item, given by its id, we want to change. Positive numbers add items, negative numbers remove items.
        /// 0 produces an invalid change info.</param>
        public ItemProviderChangeInfo CalcChangeInfo(int id, int amount)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Changes the underlying item container as describe in the change info, without taking into account the OverflowAmount.
        /// Does nothing on invalid info.
        /// </summary>
        public virtual void ExecuteChangeInfo(ItemProviderChangeInfo info)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns how many of the items of the given id the container has.
        /// 0 if no item with that id was found.
        /// </summary>
        public int Contains(int id)
        {
            throw new NotImplementedException();
        }
    }
}