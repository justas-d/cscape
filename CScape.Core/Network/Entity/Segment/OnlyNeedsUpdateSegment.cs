using CScape.Core.Data;

namespace CScape.Core.Network.Entity.Segment
{
    public sealed class OnlyNeedsUpdateSegment : IUpdateSegment
    {
        public static OnlyNeedsUpdateSegment Instance { get; } = new OnlyNeedsUpdateSegment();

        private OnlyNeedsUpdateSegment()
        {
            
        }

        public void Write(OutBlob stream)
        {
            stream.WriteBits(1, 1); // continue reading?
            stream.WriteBits(2, 0); // type
        }
    }
}