using System.Diagnostics;
using CScape.Models.Data;
using CScape.Models.Game.World;

namespace CScape.Core.Network.Entity.Segment
{
    public sealed class EntityMovementRunSegment : IUpdateSegment
    {
        private readonly Direction _dir1;
        private readonly Direction _dir2;
        private readonly bool _needsUpdate;

        public EntityMovementRunSegment(Direction dir1, Direction dir2, bool needsUpdate)
        {
            Debug.Assert(dir1 != Direction.None);
            Debug.Assert(dir2 != Direction.None);
            _dir1 = dir1;
            _dir2 = dir2;
            _needsUpdate = needsUpdate;
        }


        public void Write(OutBlob stream)
        {
            stream.WriteBits(1, 1); // continue reading?
            stream.WriteBits(2, 2); // type

            stream.WriteBits(3, (byte)_dir1);
            stream.WriteBits(3, (byte)_dir2);

            stream.WriteBits(1, _needsUpdate ? 1 : 0); // add to needs updating list
        }
    }
}