using System;
using System.Collections.Generic;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using JetBrains.Annotations;

namespace CScape.Models.Game.World
{
    public interface IPlaneOfExistence : IEnumerable<IEntityHandle>, IEquatable<IPlaneOfExistence>
    {
        /// <summary>
        /// Returns whether this PoE is the overworld.
        /// </summary>
        bool IsOverworld { get; }

        /// <summary>
        /// Returns the name of is PoE
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Checks whether the given <see cref="IEntityHandle"/> is inside this PoE.
        /// </summary>
        bool ContainsEntity([NotNull] IEntityHandle handle);

        /// <summary>
        /// Removes any dead entities from the PoE and it's regions.
        /// </summary>
        void GC();

        /// <summary>
        /// Removes an entity from the PoE without notifying the entity.
        /// </summary>
        /// <param name="owningTransform"></param>
        void RemoveEntity([NotNull] ITransform owningTransform);

        /// <summary>
        /// Adds an entity to this PoE without notifying the entity.
        /// </summary>
        void RegisterNewEntity([NotNull] ITransform transform);

        /// <summary>
        /// Returns a region at the given coordinates. If it doesn't exist, a new one is created.
        /// If x or y are negative, this method is undefined.
        /// </summary>
        [CanBeNull]
        IRegion GetRegion(int x, int y);
    }
}