using System;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.World;

namespace CScape.Core.Game.Entities.FacingData
{
    public sealed class FacingDirection : IFacingData
    {
        private readonly DirectionDelta _dir;
        private readonly ServerTransform _transform;

        public short SyncX => Convert.ToInt16((RawX * 2) + 1);
        public short SyncY => Convert.ToInt16((RawY * 2) + 1);
        public int RawX => _transform.X + _dir.X;
        public int RawY => _transform.Y + _dir.Y;

        public FacingDirection(DirectionDelta dir, ServerTransform transform)
        {
            _dir = dir;
            _transform = transform;
        }
    }
}