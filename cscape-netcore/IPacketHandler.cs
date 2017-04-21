using System;
using JetBrains.Annotations;

namespace cscape
{
    public interface IPacketHandler
    {
        int[] Handles { get; }
        void Handle([NotNull] Player player, int opcode, [NotNull] Blob packet);
    }
}