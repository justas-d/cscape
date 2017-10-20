namespace CScape.Models.Game.Message
{
    public interface IGameMessage
    {
        /// <summary>
        /// The id of the game message event.
        /// </summary>
        int EventId { get; }
    }
}
