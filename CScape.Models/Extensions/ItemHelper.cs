using System;
using CScape.Models.Game.Item;

namespace CScape.Models.Extensions
{
    public static class ItemHelper
    {
        public static long GetOverflow(this IItemDefinition def, long uncheckedAmount)
            => uncheckedAmount > def.MaxAmount ? uncheckedAmount - def.MaxAmount : 0;

        public static (int? existingIdx, int? emptyIdx) GetExistingOrEmptyIdx(
            this IItemContainer container, int id)
        {
            int? emptyIdx = null;
            int? existingIdx = null;

            for (var i = 0; i < container.Provider.Count; i++)
            {
                // handle empty items, store the first index we find just in case.
                var item = container.Provider[i];
                if (item.IsEmpty())
                {
                    if (emptyIdx == null)
                        emptyIdx = i;
                    continue;
                }

                // compare id's
                if (item.Id.ItemId == id)
                {
                    // todo : should we skip items that are fully stacked when looking for items with the same id in BasicItemManager?
                    // we found an existing item, set the existing item index and gtfo out of the loop.
                    existingIdx = i;
                    break;
                }
            }

            return (existingIdx, emptyIdx);
        }

        /*
        public static bool RemoveFromA_AddToB(
            IItemContainer containerA, int idxA,
            IItemContainer containerB)
        {
            // verify idxA
            if (IsNotInRange(idxA, containerA.Size)) return false;
            var id = containerA.Provider.GetId(idxA);

            // calc changes
            var remFromA = ItemChangeInfo.Remove(idxA);
            var addToB = containerB.CalcChangeInfo(id,
                containerA.Provider.GetAmount(idxA));

            // verify add to b
            if (!addToB.IsValid && addToB.OverflowAmount != 0) return false;

            // execute
            if (!SafeDoubleInfoExecute(
                containerA, remFromA,
                containerB, addToB)) return false;

            return true;
        }
        */

        // TODO : write tests for InterManagerSwap

        /// <summary>
        /// Swaps items between two containers without preserving the item indicies.
        /// </summary>
        /// <returns>True on success, false otherwise</returns>
        public static bool InterManagerSwap(
            IItemContainer containerA, int idxA, 
            IItemContainer containerB, int idxB)
        {
            // if idxB is null, find either an item idx with the same id or an empty slot.

            // validate indicies
            if (IsNotInRange(idxA, containerA.Provider.Count)) return false;
            if (IsNotInRange(idxB, containerB.Provider.Count)) return false;

            // get items
            var itemA = containerA.Provider[idxA];
            var itemB = containerB.Provider[idxB];

            // calc change info
            // add A to containerB
            var cAtoB = containerB.CalcChangeInfo(itemA);
            // add B to containerA
            var cBtoA = containerA.CalcChangeInfo(itemB);

            // operation is undefinied if any changeInfo's are invalid or have overflow, return false
            bool IsInvalidChangeInfo(ref ItemChangeInfo info) 
                => !info.IsValid || info.OverflowAmount != 0;

            if(IsInvalidChangeInfo(ref cAtoB)) return false;
            if(IsInvalidChangeInfo(ref cBtoA)) return false;

            // managed remove A and B
            if (!SafeDoubleInfoExecute(
                containerA, ItemChangeInfo.Remove(idxA),
                containerB, ItemChangeInfo.Remove(idxB))) return false;

            // execute change infos we calculated earlier
            if (!SafeDoubleInfoExecute(
                containerA, cBtoA,
                containerB, cAtoB)) return false;

            return true;
        }

        /// <summary>
        /// Executes two ItemChangeInfo's in such a way that change will only occur if the two of them are successful.
        /// </summary>
        /// <returns>True if both infos were executed succesfully, false otherwise.</returns>
        private static bool SafeDoubleInfoExecute(
            IItemContainer managerA, ItemChangeInfo infoA,
            IItemContainer managerB, ItemChangeInfo infoB)
        {
            // cache state of A.
            var idx = infoA.Index;
            var cacheA = new ItemChangeInfo(idx, managerA.Provider[idx], 0);

            // execute
            if (!managerA.ExecuteChangeInfo(infoA)) return false;
            if (!managerB.ExecuteChangeInfo(infoB))
            {
                // A succeeded, B didn't. Revert changes to A.
                GuaranteedExecuteInfo(managerA, cacheA);
                
                return false;
            }

            return true;
        }

        private static void GuaranteedExecuteInfo(
            IItemContainer container, ItemChangeInfo info)
        {
            // make sure we're in range.
            if(0 > info.Index || info.Index >= container.Provider.Count)
                return;

            // try managed
            if (!container.ExecuteChangeInfo(info))
            {
                // it failed, force it
                container.Provider[info.Index] = info.NewItem;
            }

        }

        private static bool IsNotInRange(int val, int max)
            => 0 > val || val >= max;

        /// <summary>
        /// Swaps items between two containers and preserves the item indicies.
        /// </summary>
        /// <returns>True on success, false otherwise</returns>
        public static bool InterManagerSwapPreserveIndex(
            IItemContainer containerA, int idxA,
            IItemContainer containerB, int idxB)
        {
            // validate indicies
            if (IsNotInRange(idxA, containerA.Provider.Count)) return false;
            if (IsNotInRange(idxB, containerB.Provider.Count)) return false;

            // get items
            var itemA = containerA.Provider[idxA];
            var itemB = containerB.Provider[idxB];

            // check ids
            if (itemA.Id== itemB.Id)
            {
                // calc stacking A into B
                long uncheckedOverflow = itemB.Amount + itemA.Amount;
                var overflow = itemB.Id.GetOverflow(uncheckedOverflow);

                if (!SafeDoubleInfoExecute(
                    // stack A into B
                    containerB,
                    new ItemChangeInfo(idxB, new ItemStack(itemB.Id, Convert.ToInt32(uncheckedOverflow - overflow)), 0),
                    // leave overflow for A
                    containerA,
                    new ItemChangeInfo(idxA, new ItemStack(itemA.Id, Convert.ToInt32(overflow)), 0)))
                {
                    return false;
                }
            }
            else
            {
                // remove A and B
                if (!SafeDoubleInfoExecute(
                    containerA, ItemChangeInfo.Remove(idxA),
                    containerB, ItemChangeInfo.Remove(idxB)))
                    return false;

                // exec swap
                if (!SafeDoubleInfoExecute(
                    containerA, new ItemChangeInfo(idxA, itemB, 0),
                    containerB, new ItemChangeInfo(idxB, itemA, 0)))

                {
                    // reverse removal
                    GuaranteedExecuteInfo(containerA, new ItemChangeInfo(idxA, itemA, 0));
                    GuaranteedExecuteInfo(containerB, new ItemChangeInfo(idxB, itemB, 0));
                }
            }

            return true;
        }
    }
}