using System;

namespace CScape.Models.Game
{
    /// <inheritdoc />
    /// <summary>
    /// Defines a representation of a position in the game world.
    /// </summary>
    public interface IPosition : IEquatable<IPosition>
    {
        /// <summary>
        /// The global X coordinate in the world.
        /// </summary>
        int X { get; }

        /// <summary>
        /// The global Y coordinate in the world.
        /// </summary>
        int Y { get; }

        /// <summary>
        /// The global Z coordinate in the world.
        /// </summary>
        int Z { get; }
    }
}