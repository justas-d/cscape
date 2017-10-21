using System;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Message
{
    public sealed class EntityMessage :IGameMessage
    {
        public int EventId { get; }

        [NotNull]
        public IEntityHandle Entity { get; }

        public static EntityMessage PlayerFollowTarget([NotNull] IEntityHandle entity)
            => new EntityMessage(entity, MessageId.NewPlayerFollowTarget);

        public static EntityMessage EnteredViewRange([NotNull] IEntityHandle entity)
            => new EntityMessage(entity, MessageId.EntityEnteredViewRange);

        public static  EntityMessage LeftViewRange([NotNull] IEntityHandle entity)
            => new EntityMessage(entity, MessageId.EntityLeftViewRange);

        private EntityMessage([NotNull] IEntityHandle entity, MessageId eventId)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
            EventId = (int)eventId;
        }
    }
}
