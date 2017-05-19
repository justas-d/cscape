using CScape.Core.Data;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    public interface IPacketDispatch
    {
        void Handle([NotNull] Player player, int opcode, [NotNull] Blob packet);

        bool CanHandle(int opcode);
    }
}