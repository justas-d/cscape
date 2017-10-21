using CScape.Models.Data;

namespace CScape.Core.Network.Entity.Segment
{
    public interface IUpdateSegment
    {
        void Write(OutBlob stream);
    }
}