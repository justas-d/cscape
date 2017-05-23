using CScape.Core.Game.Entity;
using CScape.Core.Game.Interface;
using CScape.Core.Game.Item;

namespace CScape.Dev.Tests.Impl
{
    public class MockEquippable : MockItem, IEquippableItem
    {
        public EquipSlotType Slot { get; }
        public IItemBonusDefinition Attack { get; }
        public IItemBonusDefinition Defence { get; }
        public int StrengthBonus { get; }
        public int MagicBonus { get; }
        public int RangedBonus { get; }
        public int PrayerBonus { get; }
        public AttackStyle[] Styles { get; }

        public bool CanEquip(Player player)
        {
            throw new System.NotImplementedException();
        }

        public void OnEquip(Player player, IContainerInterface container, int idx)
        {
            throw new System.NotImplementedException();
        }

        public MockEquippable(int itemId, string name, int maxAmount, bool isTradable, float weight, bool isNoted, int noteSwitchId, EquipSlotType slot, IItemBonusDefinition attack, IItemBonusDefinition defence, int strengthBonus, int magicBonus, int rangedBonus, int prayerBonus, AttackStyle[] styles) : base(itemId, name, maxAmount, isTradable, weight, isNoted, noteSwitchId)
        {
            Slot = slot;
            Attack = attack;
            Defence = defence;
            StrengthBonus = strengthBonus;
            MagicBonus = magicBonus;
            RangedBonus = rangedBonus;
            PrayerBonus = prayerBonus;
            Styles = styles;
        }
    }
}