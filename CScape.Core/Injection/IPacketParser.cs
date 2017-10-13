using System.Collections.Generic;
using CScape.Core.Data;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    public interface IPacketParser
    {
        IEnumerable<PacketMetadata> Parse([NotNull] CircularBlob stream);
    }
}