using System;
using CScape.Models.Game.Message;
using CScape.Models.Game.Skill;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Message
{
    public sealed class ExperienceGainMessage : IGameMessage
    {
        public float Experience { get; }
        [NotNull]
        public ISkillModel Skill { get; }

        public ExperienceGainMessage(float experience, [NotNull] ISkillModel skill)
        {
            Experience = experience;
            Skill = skill ?? throw new ArgumentNullException(nameof(skill));
        }

        public int EventId => MessageId.ExperienceGain;
    }
}