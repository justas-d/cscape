using System;
using JetBrains.Annotations;

namespace CScape.Models.Game.Entity
{
    public interface IEntityHandle : IEquatable<IEntityHandle>
    {
        /// <summary>
        /// The generation of the entity which this handle points to.
        /// </summary>
        int Generation { get; }

        /// <summary>
        /// The id of the entity this handle points to.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// The entity system which created the entity and this handle.
        /// </summary>
        [NotNull]
        IEntitySystem System { get; }
    }
}