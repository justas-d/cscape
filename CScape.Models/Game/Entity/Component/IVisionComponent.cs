using System.Collections.Generic;
using System.Reflection.Metadata;

namespace CScape.Models.Game.Entity.Component
{
    /// <summary>
    /// Defines a component which can resolve what the entity can see, allows us to get the entities visible entities within the given view range.
    /// </summary>
    public interface IVisionComponent
    {
        /// <summary>
        /// In tiles, the view range of the component and its entity.
        /// </summary>
        int ViewRange { get; set; }

        /// <summary>
        /// Resolves whether this entity can see given <see cref="ent"/>.
        /// If <see cref="ent"/> has an <see cref="IVisionResolver"/>, the final say is resolved using it.
        /// </summary>
        /// <returns>True if this entity can see <see cref="ent"/>, false otherwise.</returns>
        bool CanSee(IEntity ent);

        /// <summary>
        /// Returns all visible entities. The handles can be dead.
        /// </summary>
        IEnumerable<EntityHandle> GetVisibleEntities();
    }
}