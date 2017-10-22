using CScape.Core.Game.World;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Directions;
using CScape.Models.Game.World;

namespace CScape.Core.Game.Entities.Directions
{
    /// <summary>
    /// Provides directions for walking in a circle pattern.
    /// Not designed for multi-entity use.
    /// </summary>
    public sealed class CircleDirectionProvider : IDirectionsProvider
    {
        private int _idx;
        private bool _isDone = false;
        public bool SetDone() => _isDone = true;

        private readonly DirectionDelta[] _directions =
        {
            new DirectionDelta(Direction.West), 
            new DirectionDelta(Direction.SouthWest),
            new DirectionDelta(Direction.South),
            new DirectionDelta(Direction.SouthEast),
            new DirectionDelta(Direction.East),
            new DirectionDelta(Direction.NorthEast),
            new DirectionDelta(Direction.North),
            new DirectionDelta(Direction.NorthWest),
        };
        
        public GeneratedDirections GetNextDirections(IEntity ent) 
            => new GeneratedDirections(_directions[_idx++ % _directions.Length], _directions[_idx++ % _directions.Length]);
        
        public bool IsDone(IEntity entity) => _isDone;
    }
}