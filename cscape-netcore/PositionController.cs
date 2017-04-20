using System;
using System.Collections.Generic;

namespace cscape
{
    public class PositionController
    {
        public ushort X { get; private set; }
        public ushort Y { get; private set; }
        public byte Z { get; private set; }

        public int RegionX { get; private set; }
        public int RegionY { get; private set; }

        public int LocalX { get; private set; }
        public int LocalY { get; private set; }

        public bool IsRunning { get; set; }

        private readonly Queue<WorldDirection> _movementQueue = new Queue<WorldDirection>();

        /// <exception cref="ArgumentOutOfRangeException">Z cannot be larger than 4.</exception>
        public PositionController(ushort x, ushort y, byte z)
        {
            SetPosition(x, y, z);
        }

        public void ResetMovementQueue() => _movementQueue.Clear();

        public void QueueMovement(WorldDirection direction) => _movementQueue.Enqueue(direction);

        public static (int x, int y) GetDelta(WorldDirection dir)
        {
            const int dn = 1;
            const int ds = -1;
            const int dw = -1;
            const int de = 1;
            switch (dir)
            {
                case WorldDirection.None:
                    return (0, 0);
                case WorldDirection.NorthWest:
                    return (dw, dn);
                case WorldDirection.North:
                    return (0, dn);
                case WorldDirection.NorthEast:
                    return (de, dn);
                case WorldDirection.West:
                    return (dw, 0);
                case WorldDirection.East:
                    return (de, 0);
                case WorldDirection.SouthWest:
                    return (dw, ds);
                case WorldDirection.South:
                    return (0, ds);
                case WorldDirection.SouthEast:
                    return (de, ds);
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
            }
        }

        public const int MinRegionBorder = 16;
        public const int MaxRegionBorder = 88;

        /// <exception cref="ArgumentOutOfRangeException">Z cannot be larger than 4.</exception>
        public void SetPosition(ushort x, ushort y, byte z)
        {
            if (z > 4) throw new ArgumentOutOfRangeException();

            X = x;
            Y = y;
            Z = z;

            RegionX = (X >> 3) - 6;
            RegionY = (Y >> 3) - 6;

            LocalX = x - 8 * RegionX;
            LocalY = y - 8 * RegionY;
        }

        public void Update()
        {
            // no op
            if (_movementQueue.Count == 0)
                return;

            // walk
            if (IsRunning && _movementQueue.Count == 1 || !IsRunning)
            {

            }
            // run
            if (IsRunning && _movementQueue.Count >= 2)
            {

            }

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