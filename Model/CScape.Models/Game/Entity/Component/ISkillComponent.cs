using System.Collections.Generic;
using CScape.Models.Game.Skill;

namespace CScape.Models.Game.Entity.Component
{
    /// <summary>
    /// Defines a component which keep track of arbitrary skills and allows the gaining of experience.
    /// </summary>
    public interface ISkillComponent : IEntityComponent
    {
        /// <summary>
        /// The rw skill map.
        /// </summary>
        Dictionary<SkillID, ISkillModel> All { get; }

        /// <summary>
        /// Gains experience in the given <see cref="skill"/>, if it's in the <see cref="All"/> skill map.
        /// </summary>
        void GainExperience(SkillID skill, float exp);
    }
}