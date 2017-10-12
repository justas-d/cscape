using System.Collections.Generic;
using System.Linq;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Message;
using CScape.Core.Game.Interface;
using CScape.Core.Game.Item;

namespace CScape.Core.Game.Items
{
    public sealed class PlayerEquipmentContainer : IItemContainer
    {
        public const int EquipmentMaxSize = 14;

        public IList<ItemStack> Provider { get; }

        public Entities.Entity Parent { get; }

        public PlayerEquipmentContainer(Entities.Entity parent)
        {
            Parent = parent;
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

            Parent.SendMessage(
                new GameMessage(
                    null, GameMessage.Type.ItemChange, new ItemChange(this, info)));

            Parent.SendMessage(
                new GameMessage(
                    null, GameMessage.Type.EquipmentChange, null));

            return true;
        }

        public int Count(int id)
        {
            return Provider.Where(item => item.Id.ItemId == id).Sum(item => item.Amount);
        }
    }
}