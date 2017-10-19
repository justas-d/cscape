using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Models.Game.Entity
{
    public interface IEntity
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
        bool AreComponentRequirementsSatisfied();

        /// <summary>
        /// Sends a <see cref="IGameMessage"/> to this entity.
        /// </summary>
        void SendMessage([NotNull] IGameMessage message);
    }
}