using CScape.Core.Game.Entity;

namespace CScape.Dev.Tests.Internal.Impl
{
    public struct Position : IPosition
    {
        public int X { get; }
        public int Y { get; }
        public byte Z { get; }

        public Position(int x, int y, byte z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}