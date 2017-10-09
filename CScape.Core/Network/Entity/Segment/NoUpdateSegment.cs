using CScape.Core.Data;

namespace CScape.Core.Network.Entity.Segment
{
    public sealed class NoUpdateSegment : IUpdateSegment
    {
        public static NoUpdateSegment Instance { get; } = new NoUpdateSegment();

        private NoUpdateSegment()
        {
            
        }

        public void Write(OutBlob stream)
        {
            stream.WriteBits(1, 0); // continue reading?
        }
    }
}