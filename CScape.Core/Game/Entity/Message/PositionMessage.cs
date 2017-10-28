using System;
using CScape.Models.Game;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Message
{
    public sealed class PositionMessage : IGameMessage
    {
        public int EventId { get; }

        [NotNull]
        public IPosition Position { get; }

        private PositionMessage([NotNull] IPosition pos, int id)
        {
            Position = pos ?? throw new ArgumentNullException(nameof(pos));
            EventId = id;
        }
    }
}
