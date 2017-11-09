using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CScape.Models.Game.Entity
{
    /// <summary>
    /// Defines a container which stores entity components.
    /// </summary>
    public interface IEntityComponentContainer
    {
        /// <summary>
        /// Unsorted lookup by mapped type of every added component.
        /// </summary>
        [NotNull]
        IReadOnlyDictionary<Type, IEntityComponent> Lookup { get; }

        /// <summary>
        /// All mapped components sorted, by priority. Does not have to guarantee uniqueness.
        /// </summary>
        [NotNull]
        IEnumerable<IEntityComponent> GetSorted();

        /// <summary>
        /// Adds a <see cref="component"/> to the entity and maps it to <see cref="type"/>.
        /// </summary>
        /// <param name="component">The instance of the component to be added to this entity component container and mapped to <see cref="type"/>.</param>
        /// <param name="type">The <see cref="IEntityComponent"/> type which the <see cref="component"/> will be mapped to.</param>
        /// <returns>True if succesfully added, false otherwise. (component exists or something).</returns>
        bool Add([NotNull] Type type, [NotNull] IEntityComponent component);

        /// <summary>
        /// Checks whether this container contains a component mapped to <see cref="type"/>.
        /// </summary>        
        bool Contains([NotNull] Type type);

        /// <summary>
        /// Gets and returns a component mapped to <see cref="type"/>
        /// </summary>
        /// <returns><see cref="IEntityComponent "/>, if it was mapped using <see cref="Add"/>, null otherwise.</returns>
        [CanBeNull]
        IEntityComponent Get([NotNull] Type type);


        /// <summary>
        /// If it exists, removes the component mapped to <see cref="type"/>
        /// </summary>
        /// <returns>True if the component was succesfully removed, false otherwise.</returns>
        bool Remove([NotNull] Type type);
    }
}