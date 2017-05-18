using System;
using CScape.Core.Data;
using CScape.Core.Game.Entity;

namespace CScape.Core.Network.Handler
{
    public sealed class ChatPacketHandler : IPacketHandler
    {
        public int[] Handles { get; } = {4};

        public void Handle(Player player, int opcode, Blob packet)
        {
            var effect = (ChatMessage.TextEffect)packet.ReadByte();
            var color = (ChatMessage.TextColor)packet.ReadByte();

            if(!Enum.IsDefined(typeof(ChatMessage.TextColor), color))
            {
                player.Log.Debug(this, $"Invalid color: {color}");
                color = ChatMessage.TextColor.Yellow;
            }

            if (!Enum.IsDefined(typeof(ChatMessage.TextEffect), effect))
            {
                player.Log.Debug(this, $"Invalid effect: {effect}");
                effect = ChatMessage.TextEffect.None;
            }

            if (packet.TryReadString(255, out string msg))
                player.LastChatMessage = new ChatMessage(player, msg, color, effect, false);
            else
                player.Log.Warning(this, "Couldn't read chat message.");
        }
    }
}