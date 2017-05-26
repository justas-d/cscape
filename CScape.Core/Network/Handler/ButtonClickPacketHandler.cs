using CScape.Core.Data;
using CScape.Core.Game.Entity;

namespace CScape.Core.Network.Handler
{
    public class ButtonClickPacketHandler : IPacketHandler
    {
        public byte[] Handles { get; } = {185};
        public void Handle(Player player, int opcode, Blob packet)
        {
            var buttonId = packet.ReadInt16();
            var interfaceId = packet.ReadInt16();

            player.DebugMsg($"Button {buttonId} interface {interfaceId} ", ref player.DebugInterface);

            player.Interfaces.HandleButton(player, interfaceId, buttonId);
        }
    }
}