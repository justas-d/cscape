using System;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Message
{
    public sealed class EntityMessage :IGameMessage
    {
        public int EventId { get; }

        [NotNull]
        public EntityHandle Entity { get; }

        public static EntityMessage PlayerFollowTarget([NotNull] EntityHandle entity)
            => new EntityMessage(entity, MessageId.NewPlayerFollowTarget);

        public static EntityMessage EnteredViewRange([NotNull] EntityHandle entity)
            => new EntityMessage(entity, MessageId.EntityEnteredViewRange);

        public static  EntityMessage LeftViewRange([NotNull] EntityHandle entity)
            => new EntityMessage(entity, MessageId.EntityLeftViewRange);

        private EntityMessage([NotNull] EntityHandle entity, int eventId)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
            EventId = eventId;
        }
    }
}
