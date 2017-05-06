using System;
using System.Linq;
using CScape.Network.Packet;
using JetBrains.Annotations;

namespace CScape.Game.Interface
{
    public class SyncedInterfaceItemManager : InterfaceItemManager
    {
        [NotNull] private readonly IUpdatePushableInterface _interf;

        public SyncedInterfaceItemManager([NotNull] GameServer server, [NotNull] IItemProvider provider, int containerInterfaceId,
            [NotNull] IUpdatePushableInterface interf) : base(server, provider, containerInterfaceId)
        {
            _interf = interf ?? throw new ArgumentNullException(nameof(interf));

            // push clear interface update.
            // todo : push initial sync update
            interf.PushUpdate(new ClearItemInterface(containerInterfaceId));
        }

        protected override void InternalExecuteChangeInfo(ref ItemProviderChangeInfo info)
        {
        }
    }

    public abstract class InterfaceItemManager : IInterfaceItemManager
    {
        [NotNull] private readonly GameServer _server;
        private IItemDatabase Db => _server.Database.Item;

        public int Size => Provider.Items.Length;
        public int ContainerInterfaceId { get; }
        public int Count { get; private set; }
        public IItemProvider Provider { get; }

        public InterfaceItemManager([NotNull] GameServer server, [NotNull] IItemProvider provider, int containerInterfaceId)
        {
            ContainerInterfaceId = containerInterfaceId;
            _server = server ?? throw new ArgumentNullException(nameof(server));
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }
        
        public ItemProviderChangeInfo CalcChangeInfo(int id, int amount)
        {
            if(amount == 0)
                return ItemProviderChangeInfo.Invalid;

            // get definition
            var def = Db.Get(id);

            if (def == null)
            {
                _server.Log.Warning(this, $"Attempted to calc change info for undefined item id {id}");
                return ItemProviderChangeInfo.Invalid;
            }

            // figure out whether an item of the same id exists in provider.
            // if we find an empty slot during this, store it just in case we don't find an existing item.
            int? emptySlotIdx = null;
            (int idx, (int id, int amnt) copy)? existingitemCopy = null;

            for (var i = 0; i < Size; i++)
            {
                var cur = Provider.Items[i];

                // handle empty items, store the first index we find just in case.
                if (ItemHelper.IsEmpty(cur))
                {
                    if (emptySlotIdx == null)
                        emptySlotIdx = i;
                    continue;
                }

                // compare id's
                if (cur.id == id)
                {
                    // we found an existing item, set the existing item index and gtfo out of the loop.
                    existingitemCopy = (i, cur);
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
            if (existingitemCopy == null)
            {
                // because we need to add a new item, inputs that result in a remove operation cannot proceed.
                // filter out remove operations
                if (amount < 0)
                    return ItemProviderChangeInfo.Invalid;

                // check if we found an empty slot during our iteration.
                if (emptySlotIdx == null)
                {
                    // we did, generate a new 
                    var overflow = CalcOverflow(amount);
                    return new ItemProviderChangeInfo(emptySlotIdx.Value, Convert.ToInt32(amount - overflow), overflow, id);
                }
                else // we found no empty slots. in this case, it means that the container is full.
                {
                    return ItemProviderChangeInfo.Invalid;
                }
            }
            else // we found an item with the same id.
            {
                // attempt to add the given amount of the item to this slot.

                var oth = existingitemCopy.Value;

                var delta = oth.copy.amnt + amount;
                long overflow = 0;

                // no carry remove item op
                if (delta == 0)
                    return new ItemProviderChangeInfo(id, oth.copy.amnt, 0, id);

                // remove with carry
                else if (delta < 0)
                    return new ItemProviderChangeInfo(id, oth.copy.amnt, delta, id);

                // add with carry
                else if (delta > 0)
                    return new ItemProviderChangeInfo(oth.idx, Convert.ToInt32(delta - overflow), overflow, id);
                else // uhh
                {
                    _server.Log.Warning(this,
                        $"Existing item id item info operation resolve resulted in dropping through delta == 0 delta > 0 delta < 0. Delta: {delta}, id: {id}, amount: {amount}, existing amount: {oth.copy.amnt}");
                    return ItemProviderChangeInfo.Invalid;
                }
            }
        }
        
        public void ExecuteChangeInfo(ItemProviderChangeInfo info)
        {
            if (info.IsValid)
                return;

            if (info.Index < 0 || info.Index >= Provider.Items.Length)
            {
                _server.Log.Debug(this, $"Out of range index in change info: {info.Index}");
                return;
            }

            // increment counter if we're adding a new item to a slot that is empty and the item that we're adding is not empty.
            if(ItemHelper.IsEmpty(Provider.Items[info.Index]) && !ItemHelper.IsEmpty((info.ItemDefId, info.AmountDelta)))
                Count++;

            // execute
            Provider.Items[info.Index].id = info.ItemDefId;
            Provider.Items[info.Index].amount += info.AmountDelta;

            // check if slot after execution is empty, if it is, decrement our item count.
            if (ItemHelper.IsEmpty(Provider.Items[info.Index]))
            {
                Provider.Items[info.Index] = ItemHelper.EmptyItem;
                Count--;
            }

            InternalExecuteChangeInfo(ref info);
        }

        protected abstract void InternalExecuteChangeInfo(ref ItemProviderChangeInfo info);

        public int Contains(int id) 
            => Provider.Items.Where(i => i.id == id).Select(i => i.amount).Sum();
    }
}