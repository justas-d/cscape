using CScape.Models.Game;
using CScape.Models.Game.Entity.FacingData;
using CScape.Models.Game.World;

namespace CScape.Core.Game.Entities.FacingData
{
    public sealed class FacingCoordinate : IFacingState
    {
        public IPosition Coordinate { get; }

        public bool TryConvertToDelta(out DirectionDelta delta) 
        {
            delta =DirectionDelta.Noop;
            return false;
        }

        public FacingCoordinate(IPosition coordinate)
        {
            Coordinate = coordinate;
        }
    }
}