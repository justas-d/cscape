using CScape.Core.Game.Entity;
using CScape.Core.Game.Interface;
using CScape.Core.Game.Item;

namespace CScape.Basic.Model
{
    // todo : proper item model
    public class BasicItem : IEquippableItem
    {
        public int ItemId { get; }
        public string Name { get; }
        public int MaxAmount { get; }
        public bool IsTradable { get; }
        public float Weight { get; }
        public bool IsNoted { get; }
        public int NoteSwitchId { get; }

        public BasicItem(int itemId, string name, int maxAmount, bool isTradable, float weight, bool isNoted, int noteSwitchId)
        {
            ItemId = itemId;
            Name = name;
            MaxAmount = maxAmount;
            IsTradable = isTradable;
            Weight = weight;
            IsNoted = isNoted;
            NoteSwitchId = noteSwitchId;
        }

        public void UseWith(Player player, IItemManager manager, int ourIdx, IItemDefinition otherItem, int otherIndex)
        {
            player.DebugMsg($"Use [i:{ItemId}x{manager.Provider.GetAmount(ourIdx)} with [i:{otherItem}x{manager.Provider.GetAmount(ourIdx)}]", ref player.DebugItems);
        }

        public void OnAction(Player player, IItemManager manager, int index, ItemActionType type)
        {
            player.DebugMsg($"Action {type} on [i:{ItemId}x{manager.Provider.GetAmount(index)}] ", ref player.DebugItems);

            if (type == ItemActionType.Generic1)
            {
                player.DebugMsg($"Equipping {ItemId}", ref player.DebugItems);

                var info = player.Equipment.CalcChangeInfo(ItemId, manager.Provider.GetAmount(index));

                if (player.Equipment.ExecuteChangeInfo(info))
                {
                    player.DebugMsg($"Success", ref player.DebugItems);
                    manager.ExecuteChangeInfo(new ItemProviderChangeInfo(index));
                }
                else
                    player.DebugMsg($"Fail", ref player.DebugItems);
            }
        }

        public bool Equals(IItemDefinition other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return ItemId == other.ItemId;
        }

        public EquipSlotType Slot { get; } = EquipSlotType.Head;
        public IItemBonusDefinition Attack { get; } = null;
        public IItemBonusDefinition Defence { get; } = null;
        public int StrengthBonus { get; } = 1;
        public int MagicBonus { get; } = 2;
        public int RangedBonus { get; } = 3;
        public int PrayerBonus { get; } = 4;
        public AttackStyle[] Styles { get; } = null;

        public bool CanEquip(Player player) => true;

        public void OnEquip(Player player, IItemManager manager, int idx)
        {
        }
    }
}