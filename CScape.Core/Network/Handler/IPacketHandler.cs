using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Network.Handler
{
    public interface IPacketHandler
    {
        byte[] Handles { get; }
        void Handle([NotNull] Game.Entities.Entity entity, [NotNull] PacketMetadata packet);
    }
}