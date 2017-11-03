using System;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Models.Game.Interface
{
    /// <summary>
    /// Defines an interface which can be identified using an id and can receive game messages.
    /// </summary>
    public interface IGameInterface : IEquatable<IGameInterface>
    {
        /// <summary>
        /// The ID of the interface.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Shows the interfaces for the given entity.
        /// </summary>
        void ShowForEntity([NotNull] IEntity entity);

        /// <summary>
        /// Closes this interface for the given entity.
        /// </summary>
        /// <param name="entity"></param>
        void CloseForEntity([NotNull] IEntity entity);

        /// <summary>
        /// Updates this interface for the given entity.
        /// </summary>
        /// <param name="entity"></param>
        void UpdateForEntity([NotNull] IEntity entity);

        /// <summary>
        /// Receives a message that was sent for the given entity.
        /// </summary>
        void ReceiveMessage([NotNull] IEntity entity, [NotNull] IGameMessage msg);
    }
}