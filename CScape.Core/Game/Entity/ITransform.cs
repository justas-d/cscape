using CScape.Core.Game.World;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    /// <summary>
    /// Defines a way of tracking and transforming the location of server-side world entities.
    /// </summary>
    public interface ITransform : IPosition
    {
        /// <summary>
        /// The parent entity.
        /// </summary>
        [NotNull] IWorldEntity Entity { get; }

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
        /// Returns the absolute distance to the given transform.
        /// Does not take into account the z planes of both transforms.
        /// </summary>
        int AbsoluteDistanceTo([NotNull]ITransform other);

        /// <summary>
        /// Returns the maximum distances of absoulte x and y position differences 
        /// between this and the other transform.
        /// </summary>
        int MaxDistanceTo([NotNull]ITransform other);

        /// <summary>
        /// Forcibly teleports the transform to the given coordinates.
        /// </summary>
        void Teleport(IPosition pos);

        /// <summary>
        /// Forcibly teleports the transform to the given coordinates.
        /// </summary>
        void Teleport(int x, int y);

        /// <summary>
        /// Forcibly teleports the transform to the given coordinates.
        /// </summary>
        void Teleport(int x, int y, byte z);

        /// <summary>
        /// Transforms (moves) the coordinates of the transform in the given direction.
        /// TODO : Handles collision (Move)
        /// </summary>
        /// <param name="dx">Must be in rage [-1; 1]</param>
        /// <param name="dy">Must be in rage [-1; 1]</param>
        void Move(sbyte dx, sbyte dy);
    }
}