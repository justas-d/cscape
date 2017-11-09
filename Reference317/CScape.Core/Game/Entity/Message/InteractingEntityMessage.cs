using System;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.InteractingEntity;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Message
{
    public sealed class InteractingEntityMessage : IGameMessage
    {
        [NotNull]
        public IInteractingEntity Interacting { get; }
        public int EventId => (int)MessageId.NewInteractingEntity;

        public InteractingEntityMessage([NotNull] IInteractingEntity interacting)
        {
            Interacting = interacting ?? throw new ArgumentNullException(nameof(interacting));
        }
    }
}