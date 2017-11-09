using CScape.Models.Game.Entity.FacingData;
using CScape.Models.Game.Entity.InteractingEntity;
using CScape.Models.Game.World;
using JetBrains.Annotations;

namespace CScape.Models.Game.Entity.Component
{
    /// <summary>
    /// Defines the entity's position in the game world.
    /// </summary>
    public interface ITransform : IPosition, IEntityComponent
    {
        /// <summary>
        /// The facing data of the entity.
        /// </summary>z
        [NotNull]
        IFacingState FacingState { get; }

        /// <summary>
        /// The entity's interacting entity.
        /// </summary>
        [NotNull]
        IInteractingEntity InteractingEntity { get; }

        /// <summary>
        /// The direction in which this entity moved last.
        /// </summary>
        DirectionDelta LastMovedDirection { get; }

        /// <summary>
        /// Sets the facing direction for this entity.
        /// </summary>
        void SetFacingDirection([NotNull] IFacingState data);

        /// <summary>
        /// Sets the interacting entity for this entity.
        /// </summary>
        void SetInteractingEntity([NotNull] IInteractingEntity ent);


        /// <summary>
        /// The PoE in which this entity resides in.
        /// </summary>
        [NotNull]
        IPlaneOfExistence PoE { get; }

        /// <summary>
        /// The current region of the entity.
        /// </summary>
        [NotNull]
        IRegion Region { get; }

        /// <summary>
        /// Leaves the current PoE and switches to the given new one, assuming they are not the same.
        /// </summary>
        void SwitchPoE([NotNull] IPlaneOfExistence newPoe);

        /// <summary>
        /// Teleports the entity to the given coordinates.
        /// </summary>
        void Teleport(int x, int y, int z);
    }
}