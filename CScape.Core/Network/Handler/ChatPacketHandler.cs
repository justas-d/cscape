using System;
using CScape.Core.Data;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Entities.Message;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;

namespace CScape.Core.Network.Handler
{
    public sealed class ChatPacketHandler : IPacketHandler
    {
        public byte[] Handles { get; } = {4};

        public int MinimumSize { get; } = 2;

        public void Handle(Game.Entities.Entity entity, PacketMetadata packet)
        {
            var player = entity.Components.Get<PlayerComponent>();
            if (player == null)
                return;

            // min size is 2
            if (MinimumSize > packet.Data.Buffer.Length) return;

            var effect = (ChatMessage.TextEffect)packet.Data.ReadByte();
            var color = (ChatMessage.TextColor)packet.Data.ReadByte();

            // verify enum values
            if (!Enum.IsDefined(typeof(ChatMessage.TextColor), color))
            {
                entity.SystemMessage($"Invalid color: {color}", SystemMessageFlags.Debug | SystemMessageFlags.Network);
                color = ChatMessage.TextColor.Yellow;
            }

            if (!Enum.IsDefined(typeof(ChatMessage.TextEffect), effect))
            {
                entity.SystemMessage($"Invalid effect: {effect}", SystemMessageFlags.Debug | SystemMessageFlags.Network);
                effect = ChatMessage.TextEffect.None;
            }

            if (packet.Data.TryReadString(out var msg))
            {
                entity.SendMessage(
                    new GameMessage(
                        null, GameMessage.Type.ChatMessage,
                            new ChatMessage(msg, player.TitleIcon, color, effect, false)));
            }
            else
            {
                entity.SystemMessage("Couldn't read chat message.", SystemMessageFlags.Debug | SystemMessageFlags.Network);
            }
                
        }
    }
}