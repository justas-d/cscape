namespace CScape.Models.Game
{
    public struct ImmIntVec3 : IPosition
    {
        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public ImmIntVec3(IPosition pos)
        {
            X = pos.X;
            Y = pos.Y;
            Z = pos.Z;
        }

        public ImmIntVec3(int x, int y, int z)
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