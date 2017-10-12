using System;
using System.Collections.Generic;
using System.Linq;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Message;
using CScape.Core.Game.Interface;
using CScape.Core.Game.Item;
using JetBrains.Annotations;

namespace CScape.Core.Game.Items
{
    /// <summary>
    /// Defines an item container which holds items in a list fashion.
    /// When adding/removing an item, this item list container will attempt to stack
    /// it any ONE other same ItemStack, if they can be stacked without overflow.
    /// </summary>
    public sealed class ListItemContainer : IItemContainer
    {
        [NotNull]
        public Entities.Entity Parent { get; }
        public IList<ItemStack> Provider { get; }

        public ListItemContainer(
            [NotNull] Entities.Entity parent, [NotNull] IList<ItemStack> provider)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public bool Swap(int idx1, int idx2)
        {
            // check if in range
            bool IsNotInRange(int val) => 0 > val || val >= Provider.Count;

            if (IsNotInRange(idx1)) return false;
            if (IsNotInRange(idx2)) return false;

            // execute swap
            var i1 = Provider[idx1];
            var i2 = Provider[idx2];

            ExecuteChangeInfo(new ItemChangeInfo(idx1, i2, 0));
            ExecuteChangeInfo(new ItemChangeInfo(idx2, i1, 0));

            return true;
        }

        public ItemChangeInfo CalcChangeInfo(ItemStack delta)
        {
            if (delta.IsEmpty())
                return ItemChangeInfo.Invalid;
            
            // figure out whether an item of the same id exists in provider.
            // if we find an empty slot during this, store it just in case we don't find an existing item.
            var (nullExistingIdx, emptySlotIdx) = this.GetExistingOrEmptyIdx(delta.Id.ItemId);

            // we've either found an existing item idx OR have an empty slot id OR have neither of those.

            // no existing item found, must operation will result in an new item.
            if (nullExistingIdx == null)
            {
                // because we need to add a new item, inputs that result in a remove operation cannot proceed.
                // filter out remove operations
                if (delta.Amount <= 0)
                    return ItemChangeInfo.Invalid;

                // check if we found an empty slot during our iteration.
                if (emptySlotIdx != null)
                {
                    // we did, generate a new item
                    var overflow = delta.Id.GetOverflow(delta.Amount);
                    return new ItemChangeInfo(
                        emptySlotIdx.Value,
                        new ItemStack(delta.Id, (int)(delta.Amount - overflow)),
                        overflow);
                }
                else // we found no empty slots. in this case, it means that the container is full.
                {
                    return ItemChangeInfo.Invalid;
                }
            }
            else // we found an item with the same id.
            {
                // attempt to add the given amount of the item to this slot.
                var existingItem = Provider[nullExistingIdx.Value];

                var finalNewAmount = existingItem.Amount + delta.Amount;
                var overflow = delta.Id.GetOverflow(finalNewAmount);

                // no carry remove item
                if (finalNewAmount == 0)
                    return ItemChangeInfo.Remove(nullExistingIdx.Value);

                // remove with carry
                if (finalNewAmount < 0)
                    return new ItemChangeInfo(
                        nullExistingIdx.Value, ItemStack.Empty, overflow);

                // add with carry
                if (finalNewAmount > 0)
                    return new ItemChangeInfo(
                        nullExistingIdx.Value,
                        new ItemStack(existingItem.Id, (int)(finalNewAmount - overflow)),
                        overflow);

                // crap
                return ItemChangeInfo.Invalid;
            }
        }

        public bool ExecuteChangeInfo(ItemChangeInfo info)
        {
            if (!info.IsValid)
                return false;

            if (info.Index < 0 || info.Index >= Provider.Count)
                return false;

            Provider[info.Index] = info.NewItem;

            Parent.SendMessage(
                new GameMessage(
                    null, GameMessage.Type.ItemChange, new ItemChange(this, info)));

            return true;
        }

        public int Count(int id)
        {
            return Provider.Where(i => i.Id.ItemId == id).Sum(i => i.Amount);
        }
    }
}
