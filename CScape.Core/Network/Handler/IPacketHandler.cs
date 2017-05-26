using CScape.Core.Data;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Network.Handler
{
    public interface IPacketHandler
    {
        byte[] Handles { get; }
        void Handle([NotNull] Player player, int opcode, [NotNull] Blob packet);
    }
}