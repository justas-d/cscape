using CScape.Core.Game.Entities.Interface;

namespace CScape.Core.Game.Entities.Component
{
    public sealed class HealthComponent : EntityComponent
    {
        private int _health;
        private int _maxHealth;

        public override int Priority { get; }

        private int Health
        {
            get => _health;
            set
            {
                _health = value;
                CheckForDeath();
            }
        }

        private int MaxHealth
        {
            get => _maxHealth;
            set
            {
                _maxHealth = value;
                CheckForDeath();
            }
        }

        public HealthComponent(Entity parent, int maxHealth = 1, int health = 1)
            :base(parent)
        {
            MaxHealth = maxHealth;
            Health = health;
        }
        
        private void CheckForDeath()
        {
            if (0 >= Health)
            {
                Parent.SendMessage(
                    new EntityMessage(
                        this,
                        EntityMessage.EventType.JustDied,
                        null));
            }
        }
        
        public override void ReceiveMessage(EntityMessage msg)
        {
            switch (msg.Event)
            {
                case EntityMessage.EventType.TookDamage:
                {
                    var dmg = msg.AsTookDamage();
                    Health -= dmg.Damage;
                    break;
                }
                case EntityMessage.EventType.HealedHealth:
                {
                    var hp = msg.AsHealedHealth();
                    Health += hp;
                    break;
                }
            }
        }
    }
}