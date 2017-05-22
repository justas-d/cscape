using CScape.Core.Game.World;

namespace CScape.Core.Game.Entity
{
    /// <summary>
    /// Provides directions for walking in a circle pattern.
    /// </summary>
    public sealed class CircleDirectionProvider : IDirectionsProvider
    {
        private int _idx;

        private readonly (sbyte, sbyte)[] _directions =
        {
            DirectionHelper.GetDelta(Direction.West),
            DirectionHelper.GetDelta(Direction.SouthWest),
            DirectionHelper.GetDelta(Direction.South),
            DirectionHelper.GetDelta(Direction.SouthEast),
            DirectionHelper.GetDelta(Direction.East),
            DirectionHelper.GetDelta(Direction.NorthEast),
            DirectionHelper.GetDelta(Direction.North),
            DirectionHelper.GetDelta(Direction.NorthWest),
        };

        public (sbyte x, sbyte y) GetNextDir()
            => _directions[_idx++ % _directions.Length];

        public bool IsDone() => false;
        public void Dispose() { } // ignored
    }
}