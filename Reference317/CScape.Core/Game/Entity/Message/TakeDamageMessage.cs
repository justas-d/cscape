using CScape.Models.Game.Entity;

namespace CScape.Core.Game.Entity.Message
{
    public enum HitType
    {
        DefenseBlue,
        AttackRed,
        PoisonGreen,
        UnknownYellow1,
        UnknownYellow2
    }

    public sealed class TakeDamageMessage : IGameMessage
    {
        public byte Damage { get; }
        public byte CurrentHealth { get; }
        public HitType Type { get; }
        public byte MaxHealth { get; }

        public static TakeDamageMessage Zero { get; } = new TakeDamageMessage(0, 0, 0, 0);

        public TakeDamageMessage(byte damage, byte currentHealth, HitType type, byte maxHealth)
        {
            Damage = damage;
            CurrentHealth = currentHealth;
            Type = type;
            MaxHealth = maxHealth;
        }

        public int EventId => (int)MessageId.TookDamageLostHealth;
    }
}