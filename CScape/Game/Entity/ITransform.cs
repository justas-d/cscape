using CScape.Game.World;
using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    public interface ITransform
    {
        /// <summary>
        /// The parent entity.
        /// </summary>
        [NotNull] IWorldEntity Entity { get; }

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
        byte Z { get; }

        /// <summary>
        /// Returns the base coordinates of the current <see cref="ClientRegion"/>
        /// </summary>
        (int x, int y) Base { get; }

        /// <summary>
        /// Returns the client region coordinates of the current region.
        /// </summary>
        (int x, int y) ClientRegion { get; }

        /// <summary>
        /// Returns the local coordinates of the transform in the current <see cref="ClientRegion"/>
        /// </summary>
        (int x, int y) Local { get; }

        /// <summary>
        /// Returns the current PoE region this transform is stored in.
        /// </summary>
        [NotNull] Region Region { get; }

        /// <summary>
        /// The entities current PoE
        /// </summary>
        [NotNull] PlaneOfExistance PoE { get; }

        /// <summary>
        /// Cleanly switches the PoE of the entity.
        /// </summary>
        void SwitchPoE([NotNull] PlaneOfExistance newPoe);

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
        void Teleport(int x, int y);

        /// <summary>
        /// Forcibly teleports the transform to the given coordinates.
        /// </summary>
        void Teleport(int x, int y, byte z);

        /// <summary>
        /// Transforms (moves) the coordinates of the transform.
        /// </summary>
        void TransformLocals(int dx, int dy);
    }
}