namespace CScape.Models.Game
{
    public sealed class MutableIntVec3 : IPosition
    {
        public bool Equals(IPosition other)
        {
            return other.X == X &&
                   other.Y == Y &&
                   other.Z == Z;
        }

        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public int Z { get; set; } = 0;

        public MutableIntVec3()
        {
        }

        public MutableIntVec3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
