using CScape.Core.Game.Entity;

namespace CScape.Core.Game.Item
{
    public sealed class NullAttackStyle : IAttackStyle
    {
        public static NullAttackStyle Singleton { get; } = new NullAttackStyle();

        public int AttackInterval { get; } = 4;

        public void GainExperience(PlayerSkills skills, int damage) { }

        public int CalculateDamage(Player player) => 0;
    }
}