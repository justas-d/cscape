using System.Collections.Generic;
using CScape.Core.Data;
using CScape.Core.Injection;
using CScape.Core.Network.Sync;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    /// <summary>
    /// Defines an entity that exists in the world.
    /// </summary>
    public interface IWorldEntity : IEntity
    {
        ILogger Log { get; }

        /// <summary>
        /// Signals to other entities that the other entities need to re-evaluate their sight for this entitiy.
        /// </summary>
        bool NeedsSightEvaluation { get; set; }

        /// <summary>
        /// Whether this entity has been destroyed from the world or not.
        /// </summary>
        bool IsDestroyed { get; }

        /// <summary>
        /// The entities position in the game world.
        /// </summary>
        [NotNull] ITransform Transform { get; }

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

        /// <summary>
        /// Registered containers which have a reference to this object.
        /// To register an entity belonging to a container, is to subscribe
        /// to the removal of said entity from the container whenever
        /// the entity is destroyed.
        /// </summary>
        IEnumerable<IRegisteredCollection> Containers { get; }

        void RegisterContainer(IRegisteredCollection cont);
        void UnregisterContainer(IRegisteredCollection cont);
    }
}