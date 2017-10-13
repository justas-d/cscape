using CScape.Core.Game.Entity;

namespace CScape.Core.Game.Item
{
    public interface IAttackStyle
    {
        int AttackInterval { get; }
        void GainExperience(PlayerSkills skills, int damage);
        int CalculateDamage(Player player);

        /*
         *  Attack styles need to:
         *    * Control the attack interval (speed) (should change as soon as the style is selected)
         *    * Determine which stats gain exp
         *    * Add hidden levels for the damage calc
         */
    }
}