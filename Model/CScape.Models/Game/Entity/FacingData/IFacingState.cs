using CScape.Models.Game.World;
using JetBrains.Annotations;

namespace CScape.Models.Game.Entity.FacingData
{
    /// <summary>
    /// Defines an entity's facing state
    /// </summary>
    public interface IFacingState
    {
        /// <summary>
        /// The coordinate at which the entity is facing.
        /// </summary>
        [NotNull]
        IPosition Coordinate { get; }

        /// <summary>
        /// Attempts to convert this facing data into a direction delta.
        /// </summary>
        bool TryConvertToDelta(out DirectionDelta delta);
    }
}