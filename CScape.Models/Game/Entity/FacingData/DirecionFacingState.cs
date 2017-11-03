using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.World;

namespace CScape.Models.Game.Entity.FacingData
{
    public sealed class DirecionFacingState : IFacingState
    {
        private readonly DirectionDelta _dir;
        private readonly ITransform _transform;

        public IPosition Coordinate => _dir + _transform;

        public bool TryConvertToDelta(out DirectionDelta delta)
        {
            delta = _dir;
            return true;
        }

        public DirecionFacingState(DirectionDelta dir, ITransform transform)
        {
            _dir = dir;
            _transform = transform;
        }
    }
}