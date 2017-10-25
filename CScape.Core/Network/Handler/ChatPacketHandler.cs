using System;
using CScape.Core.Extensions;
using CScape.Core.Game;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;

namespace CScape.Core.Network.Handler
{
    public sealed class ChatPacketHandler : IPacketHandler
    {
        public byte[] Handles { get; } = {4};
        
        public int MinimumSize { get; } = 2;

        public void Handle(IEntity entity, PacketMessage packet)
        {
            var player = entity.GetPlayer();
            if (player == null)
                return;

            // min size is 2
            if (MinimumSize > packet.Data.Buffer.Length) return;

            var effect = (ChatMessage.TextEffect)packet.Data.ReadByte();
            var color = (ChatMessage.TextColor)packet.Data.ReadByte();

            // verify enum values
            if (!Enum.IsDefined(typeof(ChatMessage.TextColor), color))
            {
                entity.SystemMessage($"Invalid color: {color}", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Network);
                color = ChatMessage.TextColor.Yellow;
            }

            if (!Enum.IsDefined(typeof(ChatMessage.TextEffect), effect))
            {
                entity.SystemMessage($"Invalid effect: {effect}", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Network);
                effect = ChatMessage.TextEffect.None;
            }

            if (packet.Data.TryReadString(out var msg))
            {
                entity.SendMessage(
                    new ChatMessageMessage(ChatMessage.Say(msg, player, color, effect)));
            }
            else
            {
                entity.SystemMessage("Couldn't read chat message.", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Network);
            }
                
        }
    }
}