namespace CScape.Core.Game.Entities.Message
{
    public enum HitType
    {
        DefenseBlue,
        AttackRed,
        PoisonGreen,
        UnknownYellow1,
        UnknownYellow2
    }


    public class HitData
    {
        public byte Damage { get; }
        public byte CurrentHealth { get; }
        public HitType Type { get; }
        public byte MaxHealth { get; }

        public static HitData Zero { get; } = new HitData(0, 0, 0, 0);

        public HitData(byte damage, byte currentHealth, HitType type, byte maxHealth)
        {
            Damage = damage;
            CurrentHealth = currentHealth;
            Type = type;
            MaxHealth = maxHealth;
        }
    }
}