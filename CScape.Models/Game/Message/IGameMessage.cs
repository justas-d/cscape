using JetBrains.Annotations;

namespace CScape.Models.Game.Message
{
    public interface IGameMessage
    {
        /// <summary>
        /// The id of the game message event.
        /// </summary>
        int EventId { get; }

        /// <summary>
        /// The data of the event.
        /// </summary>
        [CanBeNull]
        object Data { get; }
    }
}
