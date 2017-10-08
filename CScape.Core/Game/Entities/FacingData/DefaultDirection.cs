using CScape.Core.Game.World;

namespace CScape.Core.Game.Entity
{
    public sealed class DefaultDirection : IFacingData
    {
        private readonly FacingDirection _default;

        public short SyncX => _default.SyncX;
        public short SyncY => _default.SyncY;
        public int RawX => _default.RawX;
        public int RawY => _default.RawY;

        public DefaultDirection(ServerTransform transform)
        {
            _default = new FacingDirection(new DirectionDelta(Direction.South), transform);
        }

    }
}