using System;
using JetBrains.Annotations;

namespace CScape.Models.Game.Entity
{
    public interface IEntity : IEquatable<IEntity>, IEquatable<IEntityHandle>
    {
        /// <summary>
        /// The components of this entity.
        /// </summary>
        [NotNull]
        IEntityComponentContainer Components { get; }

        /// <summary>
        /// The handle which points to this entity.
        /// </summary>
        [NotNull]
        IEntityHandle Handle { get; }

        /// <summary>
        /// The logger for this entity.
        /// </summary>
        [NotNull]
        ILogger Log { get; }

        /// <summary>
        /// The name of this entity.
        /// </summary>
        [NotNull]
        string Name { get; }

        /// <summary>
        /// The parent server of this entity.
        /// </summary>
        [NotNull]
        IGameServer Server { get; }

        /// <summary>
        /// Returns whether all the components have their RequireComponent attribute(s) satisfied.
        /// </summary>
        /// <param name="message">The error message to be set.</param>
        bool AreComponentRequirementsSatisfied(out string message);

        /// <summary>
        /// Sends a <see cref="IGameMessage"/> to this entity.
        /// </summary>
        void SendMessage([NotNull] IGameMessage message);

        /// <summary>
        /// Sends a system message.
        /// <param name="msg">The message string to be sent.</param>
        /// </summary>
        void SystemMessage([NotNull] string msg, ulong flags = (ulong)SystemMessageFlags.Normal);
    }
}