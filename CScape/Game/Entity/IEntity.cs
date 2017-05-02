using System;
using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    /// <summary>
    /// Defines an entity that can be compared to others and exists within the given server.
    /// </summary>
    public interface IEntity : IEquatable<IEntity>
    {
        /// <summary>
        /// The unique id of this entity.
        /// </summary>
        uint UniqueEntityId { get; }

        /// <summary>
        /// The server to which this entity belongs to.
        /// </summary>
        [NotNull] GameServer Server { get; }

        /// <summary>
        /// Called every update tick, if scheduled for updating.
        /// The entity is responible for scheduling it's own updates.
        /// </summary>
        void Update(MainLoop loop);
    }
}