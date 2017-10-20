using CScape.Core.Game.Entities.Message;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities
{
    public sealed class ChatMessageMessage : IGameMessage
    {
        public int EventId => MessageId.ChatMessage;
        [NotNull]
        public ChatMessage Chat { get; }

        public ChatMessageMessage(ChatMessage chat)
        {
            Chat = chat ?? throw new System.ArgumentNullException(nameof(chat));
        }
    }
}