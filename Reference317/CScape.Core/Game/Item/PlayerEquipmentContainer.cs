using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Item;
using JetBrains.Annotations;

namespace CScape.Core.Game.Item
{
    public sealed class PlayerEquipmentContainer : IItemContainer
    {
        public const int EquipmentMaxSize = 14;

        public IList<ItemStack> Provider { get; }

        public IEntity Parent { get; }

        public PlayerEquipmentContainer([NotNull] IEntity parent, [NotNull] IList<ItemStack> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            Debug.Assert(items.Count == EquipmentMaxSize);
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Provider = items;
        }

        public PlayerEquipmentContainer([NotNull] IEntity parent)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Provider = new ItemStack[EquipmentMaxSize];
        }

        private (bool status, int val) IsValidIdx(EquipSlotType slot)
        {
            var idx = (int)slot;

            if (0 > idx || idx >= EquipmentMaxSize)
                return (false, -1);

            return (true, idx);
        }


        public ItemChangeInfo CalcChangeInfo(ItemStack delta)
        {
            if (delta.IsEmpty())
                return ItemChangeInfo.Invalid;

            // only add equipables
            if (!(delta.Id is IEquippableItem def))
                return ItemChangeInfo.Invalid;

            // validate idx
            var (isValid, idx) = IsValidIdx(def.Slot);
            if (!isValid)
                return ItemChangeInfo.Invalid;

            var item = Provider[idx];

            // create new item
            if (item.IsEmpty())
            {
                // disallow remove operations
                if (delta.Amount <= 0)
                    return ItemChangeInfo.Invalid;

                // can equip and slot is empty, return info.
                return new ItemChangeInfo(idx, delta, 0);
            }

            // item exists, only allow valid info if id's match
            else if(item.Id == delta.Id)
            {
                // handle overflow
                long uncheckedAmount = item.Amount + delta.Amount;
                var overflow = item.Id.GetOverflow(uncheckedAmount);

                return new ItemChangeInfo(idx, new ItemStack(item.Id, (int)(uncheckedAmount - overflow)), overflow);
            }

            return ItemChangeInfo.Invalid;
        }

        public bool ExecuteChangeInfo(ItemChangeInfo info)
        {
            if (!info.IsValid)
                return false;

            if (info.Index < 0 || info.Index >= Provider.Count)
                return false;

            Provider[info.Index] = info.NewItem;

            Parent.SendMessage(ItemChangeMessage.InventoryChange(this, info));
            Parent.SendMessage(ItemChangeMessage.EquipmentChange(this, info));

            return true;
        }

        public int Count(int id)
        {
            return Provider.Where(item => item.Id.ItemId == id).Sum(item => item.Amount);
        }
    }
}