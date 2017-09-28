using System;
using CScape.Core.Injection;

namespace CScape.Core.Game.NewEntity
{
    public sealed class HealthComponent : IEntityComponent
    {
        public Entity Parent { get; }

        public int Health { get; private set; }
        public int MaxHealth { get; }

        public HealthComponent(Entity parent, int maxHealth, int health)
        {
            Parent = parent;
            MaxHealth = maxHealth;
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