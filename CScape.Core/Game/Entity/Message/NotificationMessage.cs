using CScape.Models.Game.Entity;

namespace CScape.Core.Game.Entity.Message
{
    public sealed class NotificationMessage : IGameMessage
    {
        public static NotificationMessage FrameEnd { get; } = new NotificationMessage(MessageId.FrameEnd);
        public static NotificationMessage FrameBegin { get; } = new NotificationMessage(MessageId.FrameBegin);
        public static NotificationMessage GC { get; } = new NotificationMessage(MessageId.GC);
        public static NotificationMessage QueuedForDeath { get; } = new NotificationMessage(MessageId.QueuedForDeath);

        public static NotificationMessage NetworkPrepare { get; } = new NotificationMessage(MessageId.NetworkPrepare);
        public static NotificationMessage NetworkSync { get; } = new NotificationMessage(MessageId.NetworkSync);
        public static NotificationMessage NetworkReinitialize { get; } = new NotificationMessage(MessageId.NetworkReinitialize);

        public static NotificationMessage JustDied { get; } = new NotificationMessage(MessageId.JustDied);

        public static NotificationMessage StopMovingAlongMovePath { get; } = new NotificationMessage(MessageId.StopMovingAlongMovePath);
        public static NotificationMessage ArrivedAtDestination { get; } = new NotificationMessage(MessageId.ArrivedAtDestination);

        public static NotificationMessage ClientRegionChanged { get; } = new NotificationMessage(MessageId.ClientRegionChanged);
        public static NotificationMessage Initialize { get; } = new NotificationMessage(MessageId.Initialize);

        public static NotificationMessage SyncLocalsToGlobals { get; } = new NotificationMessage(MessageId.SyncLocalsToGlobals);

        public int EventId { get; }

        public NotificationMessage(MessageId eventId)
        {
            EventId = (int)eventId;
        }
    }
}