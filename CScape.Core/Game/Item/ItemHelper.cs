using System;
using CScape.Core.Game.Interface;
using CScape.Core.Injection;

namespace CScape.Core.Game.Item
{
    public static class ItemHelper
    {
        public static readonly (int id, int amount) EmptyItem = (EmptyId, EmptyAmount);

        public const int EmptyId = 0;
        public const int EmptyAmount = 0;

        public static bool IsEmpty(int id, int amount) => id <= EmptyId || amount <= EmptyAmount;
        public static bool IsEmpty((int id, int amount) item) => IsEmpty(item.id, item.amount);
        public static bool IsEmptyAtIndex(this IItemProvider provider, int idx)
        {
            if (0 > idx || idx >= provider.Count) return true;
            return IsEmpty(provider.GetId(idx), provider.GetAmount(idx));
        }

        public static long CalculateOverflow(IItemDefinition def, long uncheckedAmount)
        {
            return uncheckedAmount > def.MaxAmount ? uncheckedAmount - def.MaxAmount : 0;
        }

        public static (int? existingIdx, int? emptyIdx) GetExistingOrEmptyIdx(IItemManager manager, int id)
        {
            int? emptyIdx = null;
            int? existingIdx = null;

            for (var i = 0; i < manager.Size; i++)
            {
                // handle empty items, store the first index we find just in case.
                if (manager.Provider.IsEmptyAtIndex(i))
                {
                    if (emptyIdx == null)
                        emptyIdx = i;
                    continue;
                }

                // compare id's
                if (manager.Provider.GetId(i) == id)
                {
                    // todo : should we skip items that are fully stacked when looking for items with the same id in BasicItemManager?
                    // we found an existing item, set the existing item index and gtfo out of the loop.
                    existingIdx = i;
                    break;
                }
            }

            return (existingIdx, emptyIdx);
        }


        public static bool RemoveFromA_AddToB(
            IItemManager containerA, int idxA,
            IItemManager containerB)
        {
            // verify idxA
            if (IsNotInRange(idxA, containerA.Size)) return false;
            var id = containerA.Provider.GetId(idxA);

            // calc changes
            var remFromA = ItemProviderChangeInfo.Remove(idxA);
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

        /// <summary>
        /// Swaps items between two containers without preserving the item indicies.
        /// </summary>
        /// <returns>True on success, false otherwise</returns>
        public static bool InterManagerSwap(
            IItemManager containerA, int idxA, 
            IItemManager containerB, int idxB)
        {
            // if idxB is null, find either an item idx with the same id or an empty slot.

            // validate indicies
            if (IsNotInRange(idxA, containerA.Size)) return false;
            if (IsNotInRange(idxB, containerB.Size)) return false;

            // get items
            var itemA = containerA.Provider[idxA];
            var itemB = containerB.Provider[idxB];

            // calc change info
            // add A to containerB
            var cAtoB = containerB.CalcChangeInfo(itemA.id, itemA.amount);
            // add B to containerA
            var cBtoA = containerA.CalcChangeInfo(itemB.id, itemB.amount);

            // operation is undefinied if any changeInfo's are invalid or have overflow, return false
            bool IsInvalidChangeInfo(ref ItemProviderChangeInfo info) 
                => !info.IsValid || info.OverflowAmount != 0;

            if(IsInvalidChangeInfo(ref cAtoB)) return false;
            if(IsInvalidChangeInfo(ref cBtoA)) return false;

            // managed remove A and B
            if (!SafeDoubleInfoExecute(
                containerA, ItemProviderChangeInfo.Remove(idxA),
                containerB, ItemProviderChangeInfo.Remove(idxB))) return false;

            // execute change infos we calculated earlier
            if (!SafeDoubleInfoExecute(
                containerA, cBtoA,
                containerB, cAtoB)) return false;

            return true;
        }

        private static bool SafeDoubleInfoExecute(
            IItemManager managerA, ItemProviderChangeInfo infoA,
            IItemManager managerB, ItemProviderChangeInfo infoB)
        {
            // cache state of A.
            var idx = infoA.Index;
            var cacheA = new ItemProviderChangeInfo(idx, managerA.Provider.GetAmount(idx), 0,
                managerA.Provider.GetId(idx));

            // execute
            if (!managerA.ExecuteChangeInfo(infoA)) return false;
            if (!managerB.ExecuteChangeInfo(infoB))
            {
                // A succeeded, B didn't. Revert changes to A.
                if (!managerA.ExecuteChangeInfo(cacheA))
                {
                    // managed revert failed, do it manually.
                    managerA.Provider.SetId(idx, cacheA.NewItemDefId);
                    managerA.Provider.SetAmount(idx, cacheA.NewAmount);
                }

                return false;
            }

            return true;
        }

        private static bool IsNotInRange(int val, int max)
            => 0 > val || val >= max;

        /// <summary>
        /// Swaps items between two containers and preserves the item indicies.
        /// </summary>
        /// <returns>True on success, false otherwise</returns>
        public static bool InterManagerSwapPreserveIndex(
            IItemManager containerA, int idxA,
            IItemManager containerB, int idxB,
            IItemDefinitionDatabase db)
        {
            // validate indicies
            if (IsNotInRange(idxA, containerA.Size)) return false;
            if (IsNotInRange(idxB, containerB.Size)) return false;

            // get items
            var itemA = containerA.Provider[idxA];
            var itemB = containerB.Provider[idxB];

            // check ids
            if (itemA.id == itemB.id)
            {
                // get def
                var def = db.GetAsserted(itemA.id);
                if (def == null) return false;

                // calc stacking A into B
                long uncheckedOverflow = itemB.amount + itemA.amount;
                var overflow = CalculateOverflow(def, uncheckedOverflow);

                if (!SafeDoubleInfoExecute(
                    // stack A into B
                    containerB,
                    new ItemProviderChangeInfo(idxB, Convert.ToInt32(uncheckedOverflow - overflow), 0, itemB.id),
                    // leave overflow for A
                    containerA,
                    new ItemProviderChangeInfo(idxA, Convert.ToInt32(overflow), 0, itemA.id)))

                    return false;
            }
            else
            {
                // remove A and B
                if (!SafeDoubleInfoExecute(
                    containerA, ItemProviderChangeInfo.Remove(idxA),
                    containerB, ItemProviderChangeInfo.Remove(idxB)))
                    return false;

                // exec swap
                if (!SafeDoubleInfoExecute(
                    containerA, new ItemProviderChangeInfo(idxA, itemB.amount, 0, itemB.id),
                    containerB, new ItemProviderChangeInfo(idxB, itemA.amount, 0, itemA.id)))
                    return false;
            }

            return true;
        }

        public static bool  ExecuteChangeInfo(IItemManager manager, ItemProviderChangeInfo info)
        {
            if (!info.IsValid)
                return false;

            if (info.Index < 0 || info.Index >= manager.Size)
                return false;

            // execute
            manager.Provider.SetId(info.Index, info.NewItemDefId);
            manager.Provider.SetAmount(info.Index, info.NewAmount);

            if (manager.Provider.IsEmptyAtIndex(info.Index))
            {
                manager.Provider.SetId(info.Index, EmptyId);
                manager.Provider.SetAmount(info.Index, EmptyAmount);
            }

            return true;
        }
    }
}