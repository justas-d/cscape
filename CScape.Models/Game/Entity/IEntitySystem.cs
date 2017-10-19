using System.Collections.Generic;
using CScape.Models.Game.Entity.Exceptions;
using JetBrains.Annotations;

namespace CScape.Models.Game.Entity
{
    /// <summary>
    /// Defines an entity system for creating, destroying and dereferencing entities.
    /// </summary>
    public interface IEntitySystem
    {
        /// <summary>
        /// The dictionary of all the entities and their handles available to the system. Limited to only living entities.
        /// </summary>
        IReadOnlyDictionary<IEntityHandle, IEntity> All { get; }

        /// <summary>
        /// Creates an entity with nothing but an ITransform component and gives is the name <see cref="name"/>.
        /// </summary>
        IEntityHandle Create([NotNull] string name);

        /// <summary>
        /// If alive, destroys the entity pointed to by <see cref="handle"/>
        /// <returns>True if destroyed succcesfully, false otherwise.</returns>
        /// </summary>
        bool Destroy([NotNull] IEntityHandle handle);

        /// <summary>
        /// Dereferences the entity that the given <see cref="entityHandle"/> points to.
        /// </summary>
        /// <exception cref="DestroyedEntityDereference">Thrown if the handle points to an entity that is destroyed.</exception>
        [NotNull]
        IEntity Get([NotNull] IEntityHandle entityHandle);

        /// <summary>
        /// Checks whether the entity pointed to by the handle is dead.
        /// </summary>
        /// <returns>True if dead, false otherwise.</returns>
        bool IsDead(IEntityHandle handle);
    }
}