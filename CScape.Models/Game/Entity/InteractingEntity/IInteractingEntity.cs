using JetBrains.Annotations;

namespace CScape.Models.Game.Entity.InteractingEntity
{
    /// <summary>
    /// Defines an object which allows lookup of some entity's interacting entity.
    /// </summary>
    public interface IInteractingEntity
    {
        /// <summary>
        /// The ID of the interacting entity.
        /// </summary>
        short Id { get; }

        /// <summary>
        /// A refertence to the entity interacting entity.
        /// </summary>
        [CanBeNull]
        IEntityHandle Entity { get; }
    }
}