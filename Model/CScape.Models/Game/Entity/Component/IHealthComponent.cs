namespace CScape.Models.Game.Entity.Component
{
    /// <summary>
    /// Defines a component which manages the entity's health.
    /// </summary>
    public interface IHealthComponent : IEntityComponent
    {
        /// <summary>
        /// The current health of the entity.
        /// </summary>
        int Health { get; }

        /// <summary>
        /// The max health of the entity.
        /// </summary>
        int MaxHealth { get; }

        void SetNewMaxHealth(int val);
        void SetNewHealth(int val);

        void TakeDamage(int damage, int type);
    }
}