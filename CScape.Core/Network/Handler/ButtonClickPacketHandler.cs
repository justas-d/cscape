using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Message;
using CScape.Core.Injection;

namespace CScape.Core.Network.Handler
{
    public class ButtonClickPacketHandler : IPacketHandler
    {
        public byte[] Handles { get; } = {185};

        public void Handle(Game.Entities.Entity entity, PacketMessage packet)
        {
            var buttonId = packet.Data.ReadInt16();
            var interfaceId = packet.Data.ReadInt16();

            entity.SendMessage(
                new GameMessage(
                    null, GameMessage.Type.ButtonClicked, new ButtonClickMessage(buttonId, interfaceId)));
        }
    }
}