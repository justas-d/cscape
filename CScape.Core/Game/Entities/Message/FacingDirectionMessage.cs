using System;
using CScape.Models.Game.Entity.FacingData;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Message
{
    public sealed class FacingDirectionMessage : IGameMessage
    {
        [NotNull]
        public IFacingData FacingData { get; }
        public int EventId => (int)MessageId.NewFacingDirection;

        public FacingDirectionMessage([NotNull] IFacingData facingData)
        {
            FacingData = facingData ?? throw new ArgumentNullException(nameof(facingData));
        }
    }
}