using JetBrains.Annotations;

namespace CScape.Game.Interface
{
    /// <summary>
    /// Defines an interface which manages a item container.
    /// </summary>
    public interface IContainerInterface : IApiInterface
    {
        /// <summary>
        /// The internal item manager of the container interface.
        /// </summary>
        [NotNull] IItemManager Items { get; }
    }
}