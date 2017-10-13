using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Interface;
using JetBrains.Annotations;

namespace CScape.Core.Game.Item
{
    /// <summary>
    /// Defines an item which can be equipped.
    /// </summary>
    public interface IEquippableItem : IItemDefinition
    {
        EquipSlotType Slot { get; }

        [NotNull] IEquipmentStats Attack { get; }
        [NotNull] IEquipmentStats Defence { get; }

        int StrengthBonus { get; }
        int MagicBonus { get; }
        int RangedBonus { get; }
        int PrayerBonus { get; }

        IWeaponCombatType CombatType { get; }

        /// <summary>
        /// Checks whether the given player can equip the item defined by this equippable definition.
        /// Any callbacks to the player notifying them of any inability to equip this item should be handled in this method,
        /// </summary>
        bool CanEquip([NotNull] PlayerComponent player);

        /// <summary>
        /// Called whenever the given player equips an item of this definition.
        /// </summary>
        void OnEquip([NotNull] PlayerComponent player);
    }
}