using CScape.Core.Data;

namespace CScape.Core.Network.Entity.Segment
{
    public sealed class RemoveEntitySegment : IUpdateSegment
    {
        public static RemoveEntitySegment Instance { get; } = new RemoveEntitySegment();

        private RemoveEntitySegment()
        {

        }

        public void Write(OutBlob stream)
        {
            stream.WriteBits(1, 1); // is not noop?
            stream.WriteBits(2, 3); // type
        }
    }
}