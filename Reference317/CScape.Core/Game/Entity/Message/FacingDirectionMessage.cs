using System;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.FacingData;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Message
{
    public sealed class FacingDirectionMessage : IGameMessage
    {
        [NotNull]
        public IFacingState FacingState { get; }
        public int EventId => (int)MessageId.NewFacingDirection;

        public FacingDirectionMessage([NotNull] IFacingState facingState)
        {
            FacingState = facingState ?? throw new ArgumentNullException(nameof(facingState));
        }
    }
}