using System.Diagnostics;
using CScape.Core.Game.World;

namespace CScape.Core.Game.NewEntity
{
    /// <summary>
    /// Defines valid movement data. 
    /// An appearance of this data type guarantees that it's members represent 
    /// at least walking and at most running.
    /// </summary>
    public struct MovementMetadata
    {
        public bool IsWalking { get; }

        public DirectionDelta Dir1 { get; }
        public DirectionDelta Dir2 { get; }

        /// <summary>
        /// Constructs movement data.
        /// </summary>
        /// <param name="walk">Must be a valid movement. CANNOT be a noop.</param>
        /// <param name="run">Optional, if set, movement is will be interpreted as running.</param>
        public MovementMetadata(DirectionDelta walk, DirectionDelta run)
        {
            Debug.Assert(!walk.IsNoop());

            Dir1 = walk;
            Dir2 = run;

            IsWalking = Dir2.IsNoop();
        }

        public (int x, int y) SumMovements()
        {
            return (Dir1.X + Dir2.X, Dir1.Y + Dir2.Y);
        }
    }
}