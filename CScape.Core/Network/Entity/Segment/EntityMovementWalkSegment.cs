using System.Diagnostics;
using CScape.Core.Data;
using CScape.Core.Game.World;

namespace CScape.Core.Network.Entity.Segment
{
    public sealed class EntityMovementWalkSegment : IUpdateSegment
    {
        private readonly Direction _dir;
        private readonly bool _needsUpdate;

        public EntityMovementWalkSegment(Direction dir, bool needsUpdate)
        {
            Debug.Assert(dir != Direction.None);
            _dir = dir;
            _needsUpdate = needsUpdate;
        }

        public void Write(OutBlob stream)
        {
            stream.WriteBits(1, 1); // continue reading?
            stream.WriteBits(2, 1); // type

            stream.WriteBits(3, (byte)_dir);

            stream.WriteBits(1, _needsUpdate ? 1 : 0); // add to needs updating list
        }
    }
}