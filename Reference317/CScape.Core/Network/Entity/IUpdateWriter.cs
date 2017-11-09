using CScape.Core.Network.Entity.Flag;
using CScape.Core.Network.Entity.Segment;

namespace CScape.Core.Network.Entity
{
    public interface IUpdateWriter : IUpdateSegment
    {
        bool NeedsUpdate();
        void SetFlag(IUpdateFlag flag);
    }
}