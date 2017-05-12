using System;
using CScape.Core.Game.Item;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Interface
{
    public class ItemManager : IItemManager
    {
        private readonly ILogger _log;
        private readonly IItemDefinitionDatabase _db;

        public int Size => Provider.Size;
        public IItemProvider Provider { get; }

        protected ItemManager(IServiceProvider service,[NotNull] IItemProvider provider)
        {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _log = service.ThrowOrGet<ILogger>();
            _db = service.ThrowOrGet<IItemDefinitionDatabase>();
        }

        public bool Swap(int idx1, int idx2)
        {
            // check if in range
            bool IsNotInRange(int val) => 0 > val || val >= Size;
            if (IsNotInRange(idx1)) return false;
            if (IsNotInRange(idx2)) return false;

            // execute swap
            var i1 = Provider[idx1];
            var i2 = Provider[idx2];

            ExecuteChangeInfo(new ItemProviderChangeInfo(idx1, i2.amount, 0, i2.id));
            ExecuteChangeInfo(new ItemProviderChangeInfo(idx2, i1.amount, 0, i1.id));

            return true;
        }

        public ItemProviderChangeInfo CalcChangeInfo(int id, int deltaAmount)
        {
            if(deltaAmount == 0)
                return ItemProviderChangeInfo.Invalid;

            // get definition
            var def = _db.Get(id);

            if (def == null)
            {
                _log.Warning(this, $"Attempted to calc change info for undefined item id {id}");
                return ItemProviderChangeInfo.Invalid;
            }

            // figure out whether an item of the same id exists in provider.
            // if we find an empty slot during this, store it just in case we don't find an existing item.
            int? emptySlotIdx = null;
            int? nullExistingIdx = null;

            for (var i = 0; i < Size; i++)
            {
                // handle empty items, store the first index we find just in case.
                if (Provider.IsEmptyAtIndex(i))
                {
                    if (emptySlotIdx == null)
                        emptySlotIdx = i;
                    continue;
                }

                // compare id's
                if (Provider.Ids[i] == id)
                {
                    // we found an existing item, set the existing item index and gtfo out of the loop.
                    nullExistingIdx = i;
                    break;
                }
            }

            // calculates overflow
            long CalcOverflow(int amnt)
            {
                return amnt > def.MaxAmount ? amnt - def.MaxAmount : 0;
            }

            // we've either found an existing item idx OR have an empty slot id OR have neither of those.

            // no existing item found, must operation will result in an new item.
            if (nullExistingIdx == null)
            {
                // because we need to add a new item, inputs that result in a remove operation cannot proceed.
                // filter out remove operations
                if (deltaAmount < 0)
                    return ItemProviderChangeInfo.Invalid;

                // check if we found an empty slot during our iteration.
                if (emptySlotIdx != null)
                {
                    // we did, generate a new item
                    var overflow = CalcOverflow(deltaAmount);
                    return new ItemProviderChangeInfo(emptySlotIdx.Value, Convert.ToInt32(deltaAmount - overflow), overflow, id);
                }
                else // we found no empty slots. in this case, it means that the container is full.
                {
                    return ItemProviderChangeInfo.Invalid;
                }
            }
            else // we found an item with the same id.
            {
                // attempt to add the given amount of the item to this slot.

                var existingIdx = nullExistingIdx.Value;
                var existingAmount = Provider.Amounts[existingIdx];

                var finalNewAmount = existingAmount + deltaAmount;
                var overflow = CalcOverflow(finalNewAmount);

                // no carry remove item op
                if (finalNewAmount == 0)
                    return new ItemProviderChangeInfo(existingIdx, finalNewAmount, 0, id);

                // remove with carry
                else if (finalNewAmount < 0)
                    return new ItemProviderChangeInfo(existingIdx, finalNewAmount, overflow, id);

                // add with carry
                else if (finalNewAmount > 0)
                    return new ItemProviderChangeInfo(existingIdx, Convert.ToInt32(finalNewAmount - overflow), overflow, id);
                else // uhh
                {
                    _log.Warning(this,
                        $"Existing item id item info operation resolve resulted in dropping through delta == 0 delta > 0 delta < 0. Delta: {finalNewAmount}, id: {id}, amount: {deltaAmount}, existing amount: {existingAmount}");
                    return ItemProviderChangeInfo.Invalid;
                }
            }
        }
        
        public virtual void ExecuteChangeInfo(ItemProviderChangeInfo info)
        {
            if (!info.IsValid)
                return;

            if (info.Index < 0 || info.Index >= Provider.Size)
            {
                _log.Debug(this, $"Out of range index in change info: {info.Index}");
                return;
            }

            // execute
            Provider.Ids[info.Index] = info.NewItemDefId;
            Provider.Amounts[info.Index] = info.NewAmount;

            if (Provider.IsEmptyAtIndex(info.Index))
            {
                Provider.Ids[info.Index] = ItemHelper.EmptyId;
                Provider.Amounts[info.Index] = ItemHelper.EmptyAmount;
            }
        }

        public int Contains(int id)
        {
            var ret = 0;

            for (var i = 0; i < Provider.Size; i++)
                if (Provider.Ids[i] == id) ret += Provider.Amounts[i];

            return ret;
        }
    }
}