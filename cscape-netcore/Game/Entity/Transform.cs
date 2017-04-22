using System;

namespace CScape.Game.Entity
{
    public sealed class Transform
    {
        public const int MinRegionBorder = 16;
        public const int MaxRegionBorder = 88;

        public ushort X { get; private set; }
        public ushort Y { get; private set; }
        public byte Z { get; private set; }

        public int RegionX { get; private set; }
        public int RegionY { get; private set; }

        public int LocalX { get; private set; }
        public int LocalY { get; private set; }

        /// <exception cref="ArgumentOutOfRangeException">Z cannot be larger than 4.</exception>
        public Transform(ushort x, ushort y, byte z)
        {
            SetPosition(x, y, z);
        }

        /// <exception cref="ArgumentOutOfRangeException">Z cannot be larger than 4.</exception>
        public void SetPosition(ushort x, ushort y, byte z)
        {
            if (z > 4) throw new ArgumentOutOfRangeException($"{nameof(z)} cannot be larger than 4.");

            X = x;
            Y = y;
            Z = z;

            RegionX = (X >> 3) - 6;
            RegionY = (Y >> 3) - 6;

            LocalX = x - 8 * RegionX;
            LocalY = y - 8 * RegionY;
        }

        /// <summary>
        /// Returns the abs. distance to the given transform.
        /// Does not take into account the z planes of both transforms.
        /// </summary>
        public int AbsoluteDistanceTo(Transform other)
        {
            return Math.Abs(other.X - X) + Math.Abs(other.Y - Y);
        }

        public void TransformLocals(sbyte tx, sbyte ty)
        {
            LocalX += tx;
            LocalY += ty;
        }

        public void Update()
        {
            var deltaX = 0;
            var deltaY = 0;

            if (LocalX < MinRegionBorder)
            {
                deltaX = 4 * 8;
                RegionX -= 4;
            }
            else if (LocalX >= MaxRegionBorder)
            {
                deltaX = -4 * 8;
                RegionX += 4;
            }

            if (LocalY < MinRegionBorder)
            {
                deltaY = 4 * 8;
                RegionY -= 4;
            }
            else if (LocalY >= MaxRegionBorder)
            {
                deltaY = -4 * 8;
                RegionY += 4;
            }

            LocalX += deltaX;
            LocalY += deltaY;
        }
    }
}