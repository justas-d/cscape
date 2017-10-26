using System;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Skill;

namespace CScape.Core.Database
{
    public sealed class SerializedSkillModel : ISkillModel
    {
        public SkillID Id => throw new InvalidOperationException("Skill models were not overwritten with custom ISkillModel after player has been serialized.");
        public int Boost { get; set; }
        public int Level { get; set; }
        public float Experience { get; set; }

        public bool GainExperience(IEntity ent, float exp)
        {
            throw new InvalidOperationException("Skill models were not overwritten with custom ISkillModel after player has been serialized.");   
        }
    }
}