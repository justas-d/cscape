using CScape.Models.Game.Item;

namespace CScape.Models.Game.Interface
{
    /// <summary>
    /// Defines a game interface which contains items.
    /// </summary>
    public interface IItemGameInterface : IGameInterface
    {
        /// <summary>
        /// The interface's item container.
        /// </summary>
        IItemContainer Container { get; }
    }
}