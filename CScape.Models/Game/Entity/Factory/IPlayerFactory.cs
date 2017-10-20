using System.Collections.Generic;
using System.Reflection.Metadata;
using CScape.Models.Game.Entity.Exceptions;
using JetBrains.Annotations;

namespace CScape.Models.Game.Entity.Factory
{
    /// <summary>
    /// Defines a factory for player entities.
    /// </summary>
    public interface IPlayerFactory
    {
        IEntitySystem EntitySystem { get; }
        IReadOnlyList<EntityHandle> All { get; }

        // TODO : define IPlayerFactory

        [CanBeNull]
        EntityHandle Get(int id);

        /// <summary>
        /// Creates a player entity.
        /// </summary>
        /// <param name="model">The player model which the new player entity will represent db sync with.</param>
        /// <returns>An <see cref="EntityHandle"/> pointing to the new player entity or null if the player list is full.</returns>
        /// <exception cref="EntityComponentNotSatisfied">One of the components of the entity is not satisfied</exception>
        [CanBeNull]
        EntityHandle Create([NotNull] IPlayerModel model);
    }
}