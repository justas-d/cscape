using System;
using CScape.Models.Game.Skill;

namespace CScape.Core.Database
{
    public sealed class SerializedSkillModel : ISkillModel
    {
        public SkillID Id => new SkillID("null", 0, 01);
        public int Boost { get; set; }
        public byte Level { get; set; }
        public float Experience { get; set; }
        public bool GainExperience(float exp)
        {
            throw new InvalidOperationException("Skill models were not overwritten with custom ISkillModel after player has been serialized.");
        }
    }
}