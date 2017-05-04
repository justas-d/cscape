using System;

namespace CScape.Game.World
{
    public static class DirectionHelper
    {
        public static readonly (sbyte x, sbyte y) NoopDelta = (0, 0);

        public static (sbyte x, sbyte y) Invert((sbyte x, sbyte y) dir)
        {
            dir.x *= -1;
            dir.y *= -1;
            return dir;
        }

        public static Direction Invert(Direction dir)
        {
            switch (dir)
            {
                case Direction.None:
                    return Direction.None;
                case Direction.NorthWest:
                    return Direction.SouthEast;
                case Direction.North:
                    return Direction.South;
                case Direction.NorthEast:
                    return Direction.SouthWest;
                case Direction.West:
                    return Direction.East;
                case Direction.East:
                    return Direction.West;
                case Direction.SouthWest:
                    return Direction.NorthEast;
                case Direction.South:
                    return Direction.North;
                case Direction.SouthEast:
                    return Direction.NorthWest;
            }

            throw new ArgumentOutOfRangeException(nameof(dir));
        }

        public static (sbyte x, sbyte y) GetDelta(Direction dir)
        {
            const sbyte dn = 1;
            const sbyte ds = -1;
            const sbyte dw = -1;
            const sbyte de = 1;
            switch (dir)
            {
                case Direction.None:
                    return (0, 0);
                case Direction.NorthWest:
                    return (dw, dn);
                case Direction.North:
                    return (0, dn);
                case Direction.NorthEast:
                    return (de, dn);
                case Direction.West:
                    return (dw, 0);
                case Direction.East:
                    return (de, 0);
                case Direction.SouthWest:
                    return (dw, ds);
                case Direction.South:
                    return (0, ds);
                case Direction.SouthEast:
                    return (de, ds);
            }

            throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
        }

        /// <summary>
        /// Translates deltas into directions.
        /// Domain [-1; 1] (for x y)
        /// </summary>
        public static Direction GetDirection((sbyte x, sbyte y) d)
        {
            if (d.x == 0 && d.y == 0)
                return Direction.None;

            // east
            if (d.x == 1)
            {
                if (d.y == 1)
                    return Direction.NorthEast;
                if(d.y == -1)
                    return Direction.SouthEast;
                if(d.y == 0)
                    return Direction.East;
            }
            if(d.x == -1)// west
            {
                if(d.y == 1)
                    return Direction.NorthWest;
                if(d.y == -1)
                    return Direction.SouthWest;
                if(d.y == 0)
                    return Direction.West;
            }
            // n/s
            if (d.x == 0)
            {
                if(d.y == 1)
                    return Direction.North;
                if(d.y == -1)
                    return Direction.South;
                if (d.y == 0)
                    return Direction.None;
            }

            throw new ArgumentOutOfRangeException(nameof(d), $"got undefined args: ({ d.x } { d.y})");
        }
    }
}