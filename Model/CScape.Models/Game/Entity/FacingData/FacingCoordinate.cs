using CScape.Models.Game.World;

namespace CScape.Models.Game.Entity.FacingData
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