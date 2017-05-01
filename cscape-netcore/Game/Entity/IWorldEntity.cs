using CScape.Data;
using CScape.Game.World;
using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    /// <summary>
    /// Defines an entity that exists in the world.
    /// </summary>
    public interface IWorldEntity : IEntity
    {
        /// <summary>
        /// Whether this entity has been destroyed from the world or not.
        /// </summary>
        bool IsDestroyed { get; }

        /// <summary>
        /// The entities position in the game world.
        /// </summary>
        [NotNull] Transform Position { get; }
        
        /// <summary>
        /// The entities plane of existance.
        /// </summary>
        [NotNull] PlaneOfExistance PoE { get; }

        /// <summary>
        /// Called when this entity must be synced to another observable.
        /// </summary>
        /// <param name="sync">The other entities observable sync machine.</param>
        /// <param name="isNew">Whether this is the first time the other entity is seeing this entity.</param>
        void SyncTo(ObservableSyncMachine sync, Blob stream, bool isNew);
        // todo : abstract the SyncTo proceedure so that observers that don't need a sync machine or Blob stream can be synced to.

        /// <summary>
        /// Figures out whether this entity can see the given other world entity.
        /// </summary>
        bool CanSee(IWorldEntity ent);

        /// <summary>
        /// Destroys the Entity, making sure it doesn't exist in the world any longer, frees up its id.
        /// Overriding this method should be done by overriding the virtual method 
        /// InternalDestroy(), which is called after AbstractEntity.Destroy().
        /// </summary>
        void Destroy();
    }
}