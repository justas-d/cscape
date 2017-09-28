using System;
using CScape.Core.Game.NewEntity;
using CScape.Core.Game.World;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    public static class TransformExtensions
    {
        /// <summary>
        /// Returns the absolute distance to the given transform.
        /// </summary>
        public static (int x, int y, int z) TaxicabDistanceTo(this ITransform us, [NotNull] ITransform other)
        {
            return (Math.Abs(us.X - other.X), Math.Abs(us.Y - other.Y), Math.Abs(us.Z - other.Z));
        }

        /// <summary>
        /// Returns the maximum distances of absolute x and y position differences 
        /// between this and the other transform.
        /// </summary>
        public static int ChebyshevDistanceTo(this ITransform us, [NotNull] ITransform other)
        {
            return Math.Max(Math.Abs(us.X - other.X), Math.Abs(us.Y - other.Y));
        }

        /// <summary>
        /// Forcibly teleports the transform to the given coordinates.
        /// </summary>
        public static void Teleport(this ITransform us, IPosition pos)
        {
            us.Teleport(pos.X, pos.Y, pos.Z);
        }

        /// <summary>
        /// Forcibly teleports the transform to the given coordinates.
        /// </summary>
        public static void Teleport(this ITransform us, int x, int y)
        {
            us.Teleport(x, y, us.Z);
        }
    }


    /// <summary>
    /// Defines a way of tracking and transforming the location of server-side world entities.
    /// </summary>
    public interface ITransform: IPosition, IEntityComponent
    {
        /// <summary>
        /// Returns the current PoE region this transform is stored in.
        /// </summary>
        [NotNull] Region Region { get; }

        /// <summary>
        /// The entities current PoE
        /// </summary>
        [NotNull] PlaneOfExistence PoE { get; }

        /// <summary>
        /// Cleanly switches the PoE of the entity.
        /// </summary>
        void SwitchPoE([NotNull] PlaneOfExistence newPoe);

        /// <summary>
        /// Forcibly teleports the transform to the given coordinates.
        /// </summary>
        void Teleport(int x, int y, int z);

        /// <summary>
        /// Transforms (moves) the coordinates of the transform in the given direction.
        /// TODO : Handle collision (Move)
        /// </summary>
        /// <param name="dx">Must be in rage [-1; 1]</param>
        /// <param name="dy">Must be in rage [-1; 1]</param>
        void Move(sbyte dx, sbyte dy);
    }
}