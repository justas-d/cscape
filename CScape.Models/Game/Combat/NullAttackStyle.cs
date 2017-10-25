using CScape.Models.Game.Combat;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;

namespace CScape.Models.Game.Item
{
    public sealed class NullAttackStyle : IAttackStyle
    {
        public static NullAttackStyle Instance { get; } = new NullAttackStyle();

        private NullAttackStyle()
        {
            
        }

        public int AttackInterval { get; } = 4;

        public void GainExperience(ISkillComponent skills, int damage)
        {
        }

        public int CalculateDamage(IEntity ent, IEntity target) => 0;
    }
}