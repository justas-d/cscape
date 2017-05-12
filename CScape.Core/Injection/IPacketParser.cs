using System.Collections.Generic;
using CScape.Core.Data;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    public interface IPacketParser
    {
        IEnumerable<(int Opcode, Blob Packet)> Parse([NotNull] Player player);
    }
}