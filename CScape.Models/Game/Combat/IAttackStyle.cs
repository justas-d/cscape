using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using JetBrains.Annotations;

namespace CScape.Models.Game.Combat
{
    public interface IAttackStyle
    {
        /// <summary>
        /// In ticks, the time between attacks.
        /// </summary>
        int AttackInterval { get; }

        /// <summary>
        /// Gives experience for doing n amount of damage with this style.
        /// </summary>
        void GainExperience([NotNull] ISkillComponent skills, int damage);

        /// <summary>
        /// Calculates the damage for entity <see cref="ent"/> attacking <see cref="target"/>
        /// <returns>The damage <see cref="target"/> should be delt.</returns>
        /// </summary>
        int CalculateDamage([NotNull] IEntity ent, [NotNull] IEntity target);

        /*
         *  Attack styles need to:
         *    * Control the attack interval (speed) (should change as soon as the style is selected)
         *    * Determine which stats gain exp
         *    * Add hidden levels for the damage calc
         */
    }
}