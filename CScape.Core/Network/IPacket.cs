using CScape.Models.Data;
using JetBrains.Annotations;

namespace CScape.Core.Network
{
    public interface IPacket
    {
        void Send([NotNull] OutBlob stream);
    }
}