using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    /// <summary>
    /// Defines a world entity that can move.
    /// </summary>
    public interface IMovingEntity : IWorldEntity
    {
        /// <summary>
        /// The controller which manages movement for this entity.
        /// </summary>
        [NotNull] MovementController Movement { get; }

        /// <summary>
        /// The direction in which this entity last moved in.
        /// </summary>
        (sbyte x, sbyte y) LastMovedDirection { get; set; }

        /// <summary>
        /// The world entity with which this entity is interacting with.
        /// </summary>
        [CanBeNull] IWorldEntity InteractingEntity { get; set; }

        /// <summary>
        /// Called after this entity is done moving.
        /// </summary>
        void OnMoved();
    }
}