using System;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Message
{
    public sealed class ForcedMovementMessage : IGameMessage
    {
        [NotNull]
        public ForcedMovement Movement { get; }
        public int EventId => (int)MessageId.ForcedMovement;

        public ForcedMovementMessage([NotNull] ForcedMovement movement)
        {
            Movement = movement ?? throw new ArgumentNullException(nameof(movement));
        }
    }
}
