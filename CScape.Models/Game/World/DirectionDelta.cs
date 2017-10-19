using System;

namespace CScape.Models.Game.World
{
    /// <summary>
    /// Provides an immutable tuple representing world directions as deltas using {-1; 0; 1}
    /// </summary>
    public struct DirectionDelta : IEquatable<DirectionDelta>, IEquatable<Direction>
    {
        private struct IntVec3 : IPosition
        {
            public int X { get; }
            public int Y { get; }
            public int Z { get; }

            public IntVec3(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public bool Equals(IPosition other)
            {
                return X == other.X && Y == other.Y && Z == other.Z;
            }
        }

        public Direction Direction { get; }

        public sbyte X { get; }
        public sbyte Y { get; }

        public DirectionDelta(Direction direction)
        {
            Direction = direction;
            const sbyte dn = 1;
            const sbyte ds = -1;
            const sbyte dw = -1;
            const sbyte de = 1;

            switch (direction)
            {
                case Direction.None:
                    {
                        X = 0;
                        Y = 0;
                        break;
                    }
                case Direction.NorthWest:
                    {
                        X = dw;
                        Y = dn;
                        break;
                    }

                case Direction.North:
                    {
                        X = 0;
                        Y = dn;
                        break;
                    }
                case Direction.NorthEast:
                    {
                        X = de;
                        Y = dn;
                        break;
                    }

                case Direction.West:
                    {
                        X = dw;
                        Y = 0;
                        break;
                    }

                case Direction.East:
                    {
                        X = de;
                        Y = 0;

                        break;
                    }
                case Direction.SouthWest:
                    {
                        X = dw;
                        Y = ds;
                        break;
                    }

                case Direction.South:
                    {
                        X = 0;
                        Y = ds;
                        break;
                    }

                case Direction.SouthEast:
                    {
                        X = de;
                        Y = ds;
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        /// <summary>
        /// Translates deltas into directions.
        /// Domain [-1; 1] (for x y)
        /// </summary>
        public DirectionDelta(sbyte x, sbyte y)
        {
            X = x;
            Y = y;

            if (x == 0 && y == 0)
                Direction = Direction.None;

            else switch (x)
                {
                    case 1:
                        switch (y)
                        {
                            case 1:
                                Direction = Direction.NorthEast;
                                break;
                            case -1:
                                Direction = Direction.SouthEast;
                                break;
                            case 0:
                                Direction = Direction.East;
                                break;
                        }
                        break;
                    case -1:
                        switch (y)
                        {
                            case 1:
                                Direction = Direction.NorthWest;
                                break;
                            case -1:
                                Direction = Direction.SouthWest;
                                break;
                            case 0:
                                Direction = Direction.West;
                                break;
                        }
                        break;
                    case 0:
                        switch (y)
                        {
                            case 1:
                                Direction = Direction.North;
                                break;
                            case -1:
                                Direction = Direction.South;
                                break;
                            case 0:
                                Direction = Direction.None;
                                break;
                        }
                        break;
                }

            throw new ArgumentOutOfRangeException(nameof(x), $"got undefined args: ({x} {y})");
        }

        public DirectionDelta Invert()
        {
            switch (Direction)
            {
                case Direction.None:
                    return new DirectionDelta(Direction.None);
                case Direction.NorthWest:
                    return new DirectionDelta(Direction.SouthEast);
                case Direction.North:
                    return new DirectionDelta(Direction.South);
                case Direction.NorthEast:
                    return new DirectionDelta(Direction.SouthWest);
                case Direction.West:
                    return new DirectionDelta(Direction.East);
                case Direction.East:
                    return new DirectionDelta(Direction.West);
                case Direction.SouthWest:
                    return new DirectionDelta(Direction.NorthEast);
                case Direction.South:
                    return new DirectionDelta(Direction.North);
                case Direction.SouthEast:
                    return new DirectionDelta(Direction.NorthWest);
                default:
                    throw new ArgumentOutOfRangeException(nameof(Direction));
            }
        }

        public static DirectionDelta Noop { get; } = new DirectionDelta(Direction.None);

        public bool IsNoop() => X == 0 && Y == 0;

        public bool Equals(DirectionDelta other)
        {
            return Direction == other.Direction;
        }

        public bool Equals(Direction other)
        {
            return Direction == other;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is DirectionDelta && Equals((DirectionDelta)obj);
        }

        public static IPosition operator+(DirectionDelta delta, IPosition pos)
        {
            return new IntVec3(
                pos.X + delta.X, 
                pos.Y + delta.Y, 
                pos.Z);
        }

        public override int GetHashCode()
        {
            return (int) Direction;
        }
    }
}