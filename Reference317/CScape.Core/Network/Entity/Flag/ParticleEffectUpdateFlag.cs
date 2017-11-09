using System;
using CScape.Core.Game.Entity;
using CScape.Models.Data;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Flag
{
    public sealed class ParticleEffectUpdateFlag : IUpdateFlag
    {
        [NotNull]
        public ParticleEffect Effect { get; }

        public ParticleEffectUpdateFlag([NotNull] ParticleEffect effect)
        {
            Effect = effect ?? throw new ArgumentNullException(nameof(effect));
        }

        public FlagType Type => FlagType.ParticleEffect;

        public void Write(OutBlob stream)
        {
            stream.Write16(Effect.Id);
            stream.Write16(Effect.Height);
            stream.Write16(Effect.Delay);
        }
    }
}