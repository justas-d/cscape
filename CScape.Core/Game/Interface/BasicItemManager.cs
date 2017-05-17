using System;
using System.Linq;
using CScape.Core.Game.Item;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Interface
{
    public class BasicItemManager : AbstractSyncedItemManager, ISwappableItemManager
    {
        private readonly ILogger _log;
        private readonly IItemDefinitionDatabase _db;

        public BasicItemManager(int interfaceId, IServiceProvider service,[NotNull] IItemProvider provider) : base(interfaceId, provider)
        {
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

        public override ItemProviderChangeInfo CalcChangeInfo(int id, int deltaAmount)
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
            var (nullExistingIdx, emptySlotIdx) = ItemHelper.GetExistingOrEmptyIdx(this, id);

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
                    var overflow = ItemHelper.CalculateOverflow(def, deltaAmount);
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
                var existingAmount = Provider.GetAmount(existingIdx);

                var finalNewAmount = existingAmount + deltaAmount;
                var overflow = ItemHelper.CalculateOverflow(def, finalNewAmount);

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
        
        protected override bool InternalExecuteChangeInfo(ItemProviderChangeInfo info)
            => ItemHelper.ExecuteChangeInfo(this, info);

        public override int Contains(int id) => Provider.Where(item => item.id == id).Sum(item => item.amount);
    }
}