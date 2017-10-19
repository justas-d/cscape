using System.Collections.Generic;
using JetBrains.Annotations;

namespace CScape.Models.Game.Entity.Factory
{
    public interface INpcFactory
    {
        /// <summary>
        /// Returns all alive NPC entity handles.
        /// </summary>
        [NotNull]
        IReadOnlyList<IEntityHandle> All { get; }

        // TODO : maybe replace definition id with an INpcDefinition interface?
        // TODO : define  INpcFactory.Create
        [NotNull]
        IEntityHandle Create(string name, int definitionId);

        /// <summary>
        /// Returns the <see cref="IEntityHandle"/> associated with the given instance NPC id.
        /// </summary>
        /// <returns>The NPC's <see cref="IEntitySystem"/> if an entity with that instance id exists, null otherwise.</returns>
        [CanBeNull]
        IEntityHandle Get(int id);
    }
}