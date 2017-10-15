using CScape.Core.Game.Entities.Skill;

namespace CScape.Core.Game.Entities.Message
{
    public sealed class ExperienceGainMetadata
    {
        public float Experience { get; }
        public ISkillModel Skill { get; }

        public ExperienceGainMetadata(float experience, ISkillModel skill)
        {
            Experience = experience;
            Skill = skill;
        }
    }
}