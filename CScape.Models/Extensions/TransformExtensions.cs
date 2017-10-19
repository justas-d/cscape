using System;
using CScape.Models.Game;

namespace CScape.Models.Extensions
{
    public static class TransformExtensions
    {
        /// <summary>
        /// Returns the absolute distance to the given transform.
        /// </summary>
        public static (int x, int y, int z) TaxicabDistanceTo(this IPosition us, [NotNull] IPosition other)
        {
            return (Math.Abs(us.X - other.X), Math.Abs(us.Y - other.Y), Math.Abs(us.Z - other.Z));
        }

        /// <summary>
        /// Returns the maximum distances of absolute x and y position differences 
        /// between this and the other transform.
        /// </summary>
        public static int ChebyshevDistanceTo(this IPosition us, [NotNull] IPosition other)
        {
            return Math.Max(Math.Abs(us.X - other.X), Math.Abs(us.Y - other.Y));
        }

        /// <summary>
        /// Forcibly teleports the transform to the given coordinates.
        /// </summary>
        public static void Teleport(this ServerTransform us, IPosition pos)
        {
            us.Teleport(pos.X, pos.Y, pos.Z);
        }

        /// <summary>
        /// Forcibly teleports the transform to the given coordinates.
        /// </summary>
        public static void Teleport(this ServerTransform us, int x, int y)
        {
            us.Teleport(x, y, us.Z);
        }
    }
}