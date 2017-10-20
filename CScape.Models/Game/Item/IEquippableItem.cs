using CScape.Models.Game.Combat;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Models.Game.Item
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
        /// Checks whether the given entity can equip this item.
        /// Any callbacks to the player notifying them of any inability to equip this item should be handled in this method,
        /// </summary>
        bool CanEquip([NotNull] IEntity entity);

        /// <summary>
        /// Called whenever the given entity equips an item of this definition.
        /// </summary>
        void OnEquip([NotNull] IEntity entity);
    }
}