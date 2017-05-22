namespace CScape.Core.Game.Entity
{
    public enum HitType
    {
        DefenceBlue,
        AttackRed,
        PoisonGreen,
        UnknownYellow1,
        UnknownYellow2
    }

    public class HitData
    {
        public byte Damage { get; }
        public HitType Type { get; }
        public byte CurrentHealth { get; }
        public byte MaxHealth { get; }

        public static HitData Zero { get; } = new HitData(0, 0, 0, 0);

        public HitData(byte damage, HitType type, byte currentHealth, byte maxHealth)
        {
            Damage = damage;
            Type = type;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
        }
    }
}