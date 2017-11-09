using CScape.Core.Game.Entity.Message;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Network.Handler
{
    public interface IPacketHandler
    {
        byte[] Handles { get; }
        void Handle([NotNull] IEntity entity, [NotNull] PacketMessage packet);
    }
}