using System;
using CScape.Models.Game.Entity.Directions;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Message
{
    public sealed class BeginMovePathMessage : IGameMessage
    {
        [NotNull]
        public IDirectionsProvider Directions { get; }

        public int EventId => (int) MessageId.BeginMovePath;

        public BeginMovePathMessage([NotNull] IDirectionsProvider directions)
        {
            Directions = directions ?? throw new ArgumentNullException(nameof(directions));
        }
    }
}