using System;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entity;
using CScape.Models.Data;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Flag
{
    public sealed class AnimationUpdateFlag : IUpdateFlag
    {
        [NotNull]
        public Animation Animation { get; }

        public AnimationUpdateFlag([NotNull] Animation animation)
        {
            Animation = animation ?? throw new ArgumentNullException(nameof(animation));
        }

        public FlagType Type => FlagType.Animation;

        public void Write(OutBlob stream)
        {
            stream.Write16(Animation.Id);
            stream.Write(Animation.Delay);
        }
    }
}