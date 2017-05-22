namespace CScape.Core.Game.Entity
{
    public interface IDamageable : IWorldEntity
    {
        HitData SecondaryHit { get; }
        HitData PrimaryHit { get; }

        byte MaxHealth { get; }
        byte CurrentHealth { get; }

        /// <summary>
        /// Attempts to change the entities health. Guards against overflows of <see cref="CurrentHealth"/>
        /// </summary>
        /// <param name="dAmount">How much should health change.</param>
        /// <returns>True, if the entity dies from this change, false otherwise.</returns>
        bool Damage(byte dAmount, HitType type, bool secondary);
    }
}