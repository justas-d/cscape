using System;
using CScape.Game.Item;
using JetBrains.Annotations;

namespace CScape.Game.Interface
{
    public class ItemManager : IItemManager
    {
        private IItemDatabase Db => Server.Database.Item;

        public int Size => Provider.Size;
        public GameServer Server { get; }
        public IItemProvider Provider { get; }

        public ItemManager([NotNull] GameServer server, [NotNull] IItemProvider provider)
        {
            Server = server ?? throw new ArgumentNullException(nameof(server));
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }
        
        public ItemProviderChangeInfo CalcChangeInfo(int id, int deltaAmount)
        {
            if(deltaAmount == 0)
                return ItemProviderChangeInfo.Invalid;

            // get definition
            var def = Db.Get(id);

            if (def == null)
            {
                Server.Log.Warning(this, $"Attempted to calc change info for undefined item id {id}");
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

                // todo : ItemProviderChangeInfo delta is hard to work with. Replace with a "NewAmount" var
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
                    Server.Log.Warning(this,
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
                Server.Log.Debug(this, $"Out of range index in change info: {info.Index}");
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