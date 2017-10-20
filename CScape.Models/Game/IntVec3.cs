namespace CScape.Models.Game
{
    public struct IntVec3 : IPosition
    {
        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public IntVec3(IPosition pos)
        {
            X = pos.X;
            Y = pos.Y;
            Z = pos.Z;
        }

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
}