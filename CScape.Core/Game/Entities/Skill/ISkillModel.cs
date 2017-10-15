namespace CScape.Core.Game.Entities.Skill
{
    /// <summary>
    /// Defines the data of a player's skill.
    /// </summary>
    public interface ISkillModel
    {
        /// <summary>
        /// The identity of skill that is described in this model.
        /// Used for equality checking.
        /// </summary>
        SkillID Id { get; }

        /// <summary>
        /// The name of the skill
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The chat interface which will be displayed when this a level up event is caught for this model.
        /// </summary>
        int LevelupInterfaceId { get; }

        /// <summary>
        /// The extra levels given by the boost.
        /// </summary>
        int Boost { get; set; }

        /// <summary>
        /// Experience defined level.
        /// </summary>
        byte Level { get; set; }

        /// <summary>
        /// Raw experience
        /// </summary>
        float Experience { get; set; }

        /// <summary>
        /// Handles gaining experience points. Assumes positive exp value.
        /// </summary>
        /// <return>True if leveled up, false otherwise</return>
        bool GainExperience(float exp);
    }
}