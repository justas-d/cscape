using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CScape.Models.Game.Entity
{
    /// <summary>
    /// Defines a container which stores entity components.
    /// </summary>
    public interface IEntityComponentContainer : IEnumerable<IEntityComponent>
    {
        /// <summary>
        /// Adds a <see cref="component"/> to the entity and maps it to the <see cref="IEntityComponent"/> type of <see cref="T"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="IEntityComponent"/> type which the <see cref="component"/> will be mapped to.</typeparam>
        /// <param name="component">The instance of the component to be added to this entity component container and mapped to type <see cref="T"/>.</param>
        /// <returns>True if succesfully added, false otherwise. (component exists or something).</returns>
        bool Add<T>([NotNull] T component) where T : class, IEntityComponent;
        bool Add([NotNull] Type type, [NotNull] IEntityComponent component);

        /// <summary>
        /// Gets a component mapped to <see cref="T"/> and asserts that it is not null.
        /// Will crash the server if component was not found.
        /// </summary>
        [NotNull]
        T AssertGet<T>() where T : class, IEntityComponent;

        /// <summary>
        /// Checks whether this container contains a component mapped to <see cref="T"/>.
        /// </summary>        
        bool Contains<T>() where T : class, IEntityComponent;

        bool Contains([NotNull] Type type);

        /// <summary>
        /// Gets and returns a component mapped to <see cref="T"/>
        /// </summary>
        /// <returns><see cref="T"/>, if it was mapped using <see cref="Add{T}"/>, null otherwise.</returns>
        [CanBeNull]
        T Get<T>() where T : class, IEntityComponent;

        IEntityComponent Get([NotNull] Type type);

        /// <summary>
        /// If it exists, removes the component mapped to <see cref="T"/>.
        /// </summary>
        /// <returns>True if the component was succesfully removed, false otherwise.</returns>
        bool Remove<T>() where T : class, IEntityComponent;

        bool Remove([NotNull] Type type);
    }
}