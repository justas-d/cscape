using CScape.Core.Game.Entities.Interface;
using CScape.Core.Injection;

namespace CScape.Core.Game.Entities.Fragment.Component
{
    public sealed class HealthComponent : IEntityComponent
    {
        private int _health;
        private int _maxHealth;
        public Entity Parent { get; }

        public int Priority { get; }

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
        {
            Parent = parent;
            MaxHealth = maxHealth;
            Health = health;
        }
        
        public void Update(IMainLoop loop) { }

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
        
        public void ReceiveMessage(EntityMessage msg)
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