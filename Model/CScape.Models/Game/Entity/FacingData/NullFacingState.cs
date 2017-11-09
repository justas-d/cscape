using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.World;

namespace CScape.Models.Game.Entity.FacingData
{
    public sealed class NullFacingState : IFacingState
    {
        private readonly DirecionFacingState _default;

        public IPosition Coordinate => _default.Coordinate;

        public bool TryConvertToDelta(out DirectionDelta delta) => _default.TryConvertToDelta(out delta);

        public NullFacingState(ITransform transform)
        {
            _default = new DirecionFacingState(new DirectionDelta(Direction.South), transform);
        }
    }
}