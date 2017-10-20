using System.Collections.Generic;
using CScape.Core.Data;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    public interface IPacketParser
    {
        IEnumerable<PacketMessage> Parse([NotNull] CircularBlob stream);
    }
}