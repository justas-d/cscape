using CScape.Data;
using JetBrains.Annotations;

namespace CScape.Network.Packet
{
    public interface IPacketHandler
    {
        int[] Handles { get; }
        void Handle([NotNull] Game.Entity.Player player, int opcode, [NotNull] Blob packet);
    }
}