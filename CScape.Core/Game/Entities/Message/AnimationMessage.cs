using System;
using CScape.Core.Game.Entities.Message;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities
{
    public sealed class AnimationMessage : IGameMessage
    {
        [NotNull]
        public Animation Animation { get; }
        public int EventId => MessageId.NewAnimation;

        public AnimationMessage([NotNull] Animation animation)
        {
            Animation = animation ?? throw new ArgumentNullException(nameof(animation));
        }
    }
}