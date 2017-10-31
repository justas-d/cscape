using CScape.Models.Game;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Entity.FacingData;
using CScape.Models.Game.World;

namespace CScape.Core.Game.Entities.FacingData
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