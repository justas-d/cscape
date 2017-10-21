using CScape.Core.Game.Entities.FacingData;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.World;

namespace CScape.Models.Game.Entity.FacingData
{
    public sealed class NullFacingData : IFacingData
    {
        private readonly FacingDirection _default;

        public short SyncX => _default.SyncX;
        public short SyncY => _default.SyncY;
        public int RawX => _default.RawX;
        public int RawY => _default.RawY;

        public NullFacingData(ITransform transform)
        {
            _default = new FacingDirection(new DirectionDelta(Direction.South), transform);
        }

    }
}