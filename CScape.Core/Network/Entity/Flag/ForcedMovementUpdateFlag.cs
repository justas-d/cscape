using System;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Message;
using CScape.Models.Data;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Flag
{
    public sealed class ForcedMovementUpdateFlag : IUpdateFlag
    {
        [NotNull]
        public ForcedMovement Movement { get; }

        public ForcedMovementUpdateFlag([NotNull] ForcedMovement movement)
        {
            Movement = movement ?? throw new ArgumentNullException(nameof(movement));
        }

        public FlagType Type => FlagType.ForcedMovement;

        public void Write(OutBlob stream)
        {
            stream.Write(Movement.Start.x);
            stream.Write(Movement.Start.y);
            stream.Write(Movement.End.x);
            stream.Write(Movement.End.y);
            stream.Write(Movement.Duration.x);
            stream.Write(Movement.Duration.y);
            stream.Write((byte)Movement.Direction);
        }
    }
}