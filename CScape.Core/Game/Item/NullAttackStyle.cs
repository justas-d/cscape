using CScape.Core.Game.Entity;

namespace CScape.Core.Game.Item
{
    public sealed class NullAttackStyle : IAttackStyle
    {
        public static NullAttackStyle Instance { get; } = new NullAttackStyle();

        private NullAttackStyle()
        {
            
        }

        public int AttackInterval { get; } = 4;

        public void GainExperience(PlayerSkills skills, int damage) { }

        public int CalculateDamage(Player player) => 0;
    }
}