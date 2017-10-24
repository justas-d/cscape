using System;
using CScape.Models.Extensions;
using CScape.Models.Game;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Message
{
    public sealed class TeleportMessage : IGameMessage
    {
        [NotNull]
        public IPosition OldPos { get; }
        [NotNull]
        public IPosition NewPos { get; }

        public TeleportMessage([NotNull] IPosition oldPos, [NotNull] IPosition newPos)
        {
            if (oldPos == null) throw new ArgumentNullException(nameof(oldPos));
            if (newPos == null) throw new ArgumentNullException(nameof(newPos));
            OldPos = oldPos.Copy();
            NewPos = newPos.Copy();
        }

        public int EventId => (int)MessageId.Teleport;
    }
}