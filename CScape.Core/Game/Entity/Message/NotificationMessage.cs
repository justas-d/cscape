using CScape.Models.Game.Message;

namespace CScape.Core.Game.Entity.Message
{
    public sealed class NotificationMessage : IGameMessage
    {
        public static NotificationMessage FrameEnd { get; } = new NotificationMessage(SysMessage.FrameEnd);
        public static NotificationMessage FrameUpdate { get; } = new NotificationMessage(SysMessage.FrameUpdate);
        public static NotificationMessage GC { get; } = new NotificationMessage(SysMessage.GC);
        public static NotificationMessage DestroyEntity { get; } = new NotificationMessage(SysMessage.DestroyEntity);

        public static NotificationMessage NetworkUpdate { get; } = new NotificationMessage(MessageId.NetworkUpdate);
        public static NotificationMessage DatabaseUpdate { get; } = new NotificationMessage(MessageId.DatabaseUpdate);
        public static NotificationMessage NetworkReinitialize { get; } = new NotificationMessage(MessageId.NetworkReinitialize);

        public static NotificationMessage JustDied { get; } = new NotificationMessage(MessageId.JustDied);

        public static NotificationMessage BeginMovePath { get; } = new NotificationMessage(MessageId.BeginMovePath);
        public static NotificationMessage StopMovingAlongMovePath { get; } = new NotificationMessage(MessageId.StopMovingAlongMovePath);
        public static NotificationMessage ArrivedAtDestination { get; } = new NotificationMessage(MessageId.ArrivedAtDestination);

        public static NotificationMessage ClientRegionChanged { get; } = new NotificationMessage(MessageId.ClientRegionChanged);
        public static NotificationMessage PlayerInitialize { get; } = new NotificationMessage(MessageId.PlayerInitialize);

        public int EventId { get; }

        public NotificationMessage(MessageId eventId)
        {
            EventId = (int)eventId;
        }

        public NotificationMessage(int eventId)
        {
            EventId = eventId;
        }
    }
}