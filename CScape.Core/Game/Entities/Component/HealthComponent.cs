using CScape.Core.Game.Entities.Interface;
using CScape.Core.Injection;

namespace CScape.Core.Game.Entities.Component
{
    public sealed class HealthComponent : IEntityComponent
    {
        private int _health;
        private int _maxHealth;
        public Entity Parent { get; }

        public int Health
        {
            get => _health;
            set
            {
                _health = value;
                CheckForDeath();
            }
        }

        public int MaxHealth
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
            _maxHealth = maxHealth;
            Health = health.Clamp(0, maxHealth);

            CheckForDeath();
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
            if (msg.Event == EntityMessage.EventType.TookDamage)
            {
                var dmg = msg.AsTookDamage();
                Health -= dmg;

                CheckForDeath();
            }
        }
    }
}