using CScape.Core.Game.Entities;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    public interface IPacketDispatch
    {
        void Handle([NotNull] Entity entity, [NotNull] PacketMetadata packet);

        bool CanHandle(byte opcode);
    }
}