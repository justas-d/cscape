namespace CScape.Models.Game.Entity.Component
{
    public interface ICombatStatComponent
    {
        IEquipmentStats Attack { get; }
        IEquipmentStats Defense { get; }
        int MagicBonus { get; }
        int PrayerBonus { get; }
        int RangedBonus { get; }
        int StrengthBonus { get; }
    }
}