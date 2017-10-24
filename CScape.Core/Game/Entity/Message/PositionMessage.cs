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

        public static PositionMessage ClientRegionChange([NotNull]IPosition pos)
            => new PositionMessage(pos, (int)MessageId.ClientRegionChanged);

        private PositionMessage([NotNull] IPosition pos, int id)
        {
            Position = pos ?? throw new ArgumentNullException(nameof(pos));
            EventId = id;
        }
    }
}
