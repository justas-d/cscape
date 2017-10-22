using JetBrains.Annotations;

namespace CScape.Core.Game
{
    public sealed class ChatMessage
    {
        public enum TextColor : byte
        {
            Yellow = 0,
            Red = 1,
            Green = 2,
            Cyan = 3,
            Purple = 4,
            White = 5,
            Flash1 = 6,
            Flash2 = 7,
            Flash3 = 8,
            Glow1 = 9,
            Glow2 = 10,
            Glow3 = 11
        }

        public enum TextEffect : byte
        {
            None = 0,
            Wave = 1,
            Wave2 = 2,
            Shake = 3,
            Scroll = 4,
            Slide = 5
        }
       
        [NotNull]
        public string Message { get; }
        public int Title { get; }

        public TextColor Color { get; }
        public TextEffect Effects { get; }

        /// <summary>
        /// Whether or not this chat message was forced by the server, that is, whether the server(true) or the player(false) sent it.
        /// </summary>
        public bool IsForced { get; }

        public ChatMessage([NotNull] string message, int title, TextColor color, TextEffect effects, bool isForced)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new System.ArgumentException("message", nameof(message));
            }

            Message = message;
            Title = title;
            Color = color;
            Effects = effects;
            IsForced = isForced;
        }
    }
}