using System;
using CScape.Core.Game.Entities.Message;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities
{
    public sealed class ParticleEffectMessage : IGameMessage
    {
        [NotNull]
        public ParticleEffect Effect { get; }
        public int EventId => MessageId.ParticleEffect;

        public ParticleEffectMessage([NotNull] ParticleEffect effect)
        {
            Effect = effect ?? throw new ArgumentNullException(nameof(effect));
        }
    }
}