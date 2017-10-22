using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities
{
    public sealed class OverheadTextMessage : IGameMessage
    {
        public int EventId => (int)MessageId.NewOverheadText;
        [NotNull]
        public string Message { get; }

        public OverheadTextMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new System.ArgumentException("message", nameof(message));
            }

            Message = message;
        }
    }
}