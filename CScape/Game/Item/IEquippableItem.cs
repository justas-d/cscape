using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Game.Item
{
    /// <summary>
    /// Defines an item which can be equipped.
    /// </summary>
    public interface IEquippableItem : IItemDefinition
    {
        EquipSlotType Slot { get; }

        [NotNull] IItemBonusDefinition Attack { get; }
        [NotNull] IItemBonusDefinition Defence { get; }

        int StrengthBonus { get; }
        int MagicBonus { get; }
        int RangedBonus { get; }
        int PrayerBonus { get; }

        [CanBeNull] AttackStyle[] Styles { get; }

        bool CanEquip([NotNull] Player player);
    }
}