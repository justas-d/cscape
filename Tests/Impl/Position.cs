using CScape.Models.Game;

namespace CScape.Dev.Tests.Impl
{
    public struct Position : IPosition
    {
        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public Position(int x, int y, byte z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool Equals(Position other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public bool Equals(IPosition other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Position && Equals((Position) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X;
                hashCode = (hashCode * 397) ^ Y;
                hashCode = (hashCode * 397) ^ Z;
                return hashCode;
            }
        }
    }
}