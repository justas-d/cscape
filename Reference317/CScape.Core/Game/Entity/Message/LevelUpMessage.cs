using System;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Skill;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Message
{
    public sealed class LevelUpMessage : IGameMessage
    {
        [NotNull]
        public ISkillModel Skill { get; }
        public int EventId => (int)MessageId.LevelUp;

        public LevelUpMessage([NotNull] ISkillModel skill)
        {
            Skill = skill ?? throw new ArgumentNullException(nameof(skill));
        }
    }
}