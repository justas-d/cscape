using CScape.Models.Game.Combat;

namespace CScape.Models.Game.Entity.Component
{
    /// <summary>
    /// Defines a component which defines the entity's combat stats.
    /// </summary>
    public interface ICombatStatComponent : IEntityComponent
    {
        IEquipmentStats Attack { get; }
        IEquipmentStats Defense { get; }

        int MagicBonus { get; }
        int PrayerBonus { get; }
        int RangedBonus { get; }
        int StrengthBonus { get; }
    }
}