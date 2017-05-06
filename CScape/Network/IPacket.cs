using CScape.Data;
using JetBrains.Annotations;

namespace CScape.Network
{
    public interface IPacket
    {
        void Send([NotNull] OutBlob stream);
    }
}