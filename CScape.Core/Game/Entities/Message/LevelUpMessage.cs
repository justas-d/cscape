using System;
using CScape.Models.Game.Message;
using CScape.Models.Game.Skill;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities
{
    public sealed class LevelUpMessage : IGameMessage
    {
        [NotNull]
        public ISkillModel Skill { get; }
        public int EventId => MessageId.LevelUp;

        public LevelUpMessage([NotNull] ISkillModel skill)
        {
            Skill = skill ?? throw new ArgumentNullException(nameof(skill));
        }
    }
}