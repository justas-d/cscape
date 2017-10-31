using CScape.Models.Game.World;

namespace CScape.Models.Game.Entity.FacingData
{
    /// <summary>
    /// Defines an entities facing direction.
    /// </summary>
    public interface IFacingData
    {
        short SyncX { get; }
        short SyncY { get; }

        int RawX { get; }
        int RawY { get; }

        /// <summary>
        /// Attempts to convert this facing data into a direction delta.
        /// </summary>
        bool TryConvertToDelta(out DirectionDelta delta);
    }
}