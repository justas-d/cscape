using JetBrains.Annotations;

namespace CScape.Models.Game.Entity.Directions
{
    public interface IDirectionsProvider
    {
        /// <summary>
        /// Returns movement directions for the given entity.
        /// </summary>
        /// <param name="ent">The entity for which to calculate the next set of directions.</param>
        GeneratedDirections GetNextDirections([NotNull] IEntity ent);

        /// <summary>
        /// Returns whether the directions provider is done giving directions for the given entity.
        /// </summary>
        bool IsDone([NotNull] IEntity entity);
    }
}