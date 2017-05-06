using CScape.Data;
using JetBrains.Annotations;

namespace CScape.Network.Handlers
{
    public interface IPacketHandler
    {
        int[] Handles { get; }
        void Handle([NotNull] Game.Entity.Player player, int opcode, [NotNull] Blob packet);
    }
}