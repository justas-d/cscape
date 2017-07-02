using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    /// <summary>
    /// Defines a world entity that observes and tracks other entities.
    /// </summary>
    public interface IObserver : IWorldEntity
    {
        /// <summary>
        /// The observatory which keeps track of world entities that this IObserver can see.
        /// </summary>
        [NotNull] IObservatory Observatory { get; }

        bool IsEntityInViewRange(IWorldEntity ent);
    }
}