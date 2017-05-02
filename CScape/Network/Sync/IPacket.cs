using CScape.Data;
using JetBrains.Annotations;

namespace CScape.Network.Sync
{
    public interface IPacket
    {
        void Send([NotNull] OutBlob stream);
    }
}