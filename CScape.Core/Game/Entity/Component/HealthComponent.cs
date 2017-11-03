using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;

namespace CScape.Core.Game.Entity.Component
{
    public sealed class HealthComponent : EntityComponent, IHealthComponent
    {
        private int _health;
        private int _maxHealth;

        public int Health => _health;
        public int MaxHealth => _maxHealth;

        public override int Priority => (int)ComponentPriority.HealthComponent;

        public HealthComponent(IEntity parent, int maxHealth = 1, int health = 1)
            : base(parent)
        {
            _health = maxHealth;
            _maxHealth = health;
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case (int)MessageId.TookDamageLostHealth:
                {
                    var dmg = msg.AsTookDamangeLostHealth();
                    _health -= dmg.Damage;
                    CheckForDeath();
                    
                    break;
                }
                case (int)MessageId.EatHealedHealth:
                {
                    var hp = msg.AsEatHealed();
                    _health += hp.HealedAmount;
                    CheckForDeath();
                    
                    break;
                }
            }
        }

        public void SetNewMaxHealth(int val)
        {
            if (MaxHealth == val)
                return;

            var old = _maxHealth;
            _maxHealth = val;

            Parent.SendMessage(new MaxHealthChangedMessage(old, val));

            CheckForDeath();
        }

        public void SetNewHealth(int val)
        {
            if (Health == val)
                return;

            var old = _health;

            Parent.SendMessage(new HealthUpdateMessage(old, val));

            _health = val;
            CheckForDeath();
        }

        public void TakeDamage(int damage, int type)
        {
            byte ClampCast(int val) => (byte) val.Clamp(0, byte.MaxValue);
            Parent.SendMessage(new TakeDamageMessage(ClampCast(damage), ClampCast(Health), (HitType)type, ClampCast(MaxHealth)));
        }

        private void CheckForDeath()
        {
            if (0 >= Health)
                Parent.SendMessage(NotificationMessage.JustDied);
        }
        
    }
}