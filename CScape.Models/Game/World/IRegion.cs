using System.Collections.Generic;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using JetBrains.Annotations;

namespace CScape.Models.Game.World
{
    public interface IRegion
    {
        /// <summary>
        /// All of the entities inside this region. Handles may be dead.
        /// </summary>
        IReadOnlyCollection<IEntityHandle> Entities { get; }

        /// <summary>
        /// The parent, PoE.
        /// </summary>
        IPlaneOfExistence Poe { get; }

        /// <summary>
        /// The X, in region, coordinates of this region.
        /// </summary>
        int X { get; }

        /// <summary>
        /// The X, in region, coordinates of this region.
        /// </summary>
        int Y { get; }
        
        /// <summary>
        /// Removes any dead entities from the region.
        /// </summary>
        void GC();

        /// <summary>
        /// Returns the 8 nearby regions as well as this one. 
        /// Using (x;y) where (x;y) = (X + a; Y + b), a,b ∈ {-1,0,1}
        /// </summary>
        IEnumerable<IRegion> GetNearbyInclusive();

        /// <summary>
        /// Adds an entity to the region.
        /// </summary>
        void AddEntity([NotNull] ITransform owningTransform);

        /// <summary>
        /// Removes an entity to the region.
        /// </summary>
        void RemoveEntity([NotNull] ITransform owningTransform);

    }
}