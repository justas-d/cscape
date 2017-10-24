using System;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Message
{
    public sealed class ParticleEffectMessage : IGameMessage
    {
        [NotNull]
        public ParticleEffect Effect { get; }
        public int EventId => (int)MessageId.ParticleEffect;

        public ParticleEffectMessage([NotNull] ParticleEffect effect)
        {
            Effect = effect ?? throw new ArgumentNullException(nameof(effect));
        }
    }
}