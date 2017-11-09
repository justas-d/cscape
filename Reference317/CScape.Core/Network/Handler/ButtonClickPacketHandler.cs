using CScape.Core.Game.Entity.Message;
using CScape.Models.Game.Entity;

namespace CScape.Core.Network.Handler
{
    public class ButtonClickPacketHandler : IPacketHandler
    {
        public byte[] Handles { get; } = {185};

        public void Handle(IEntity entity, PacketMessage packet)
        {
            var buttonId = packet.Data.ReadInt16();
            var interfaceId = packet.Data.ReadInt16();

            entity.SendMessage(new ButtonClickMessage(buttonId, interfaceId));
        }
    }
}