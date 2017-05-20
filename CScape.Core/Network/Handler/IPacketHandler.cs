using CScape.Core.Data;
using JetBrains.Annotations;

namespace CScape.Core.Network.Handler
{
    public interface IPacketHandler
    {
        byte[] Handles { get; }
        void Handle([NotNull] Game.Entity.Player player, int opcode, [NotNull] Blob packet);
    }
}