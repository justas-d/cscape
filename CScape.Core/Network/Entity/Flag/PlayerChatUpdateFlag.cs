using System;
using CScape.Core.Game;
using CScape.Core.Game.Entities.Message;
using CScape.Models.Data;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Flag
{
    public sealed class PlayerChatUpdateFlag : IUpdateFlag
    {
        [NotNull]
        public ChatMessage Chat { get; }

        public PlayerChatUpdateFlag([NotNull] ChatMessage chat)
        {
            Chat = chat ?? throw new ArgumentNullException(nameof(chat));
        }

        public FlagType Type => FlagType.ChatMessage;

        public void Write(OutBlob stream)
        {
            stream.Write((byte)Chat.Color);
            stream.Write((byte)Chat.Effects);
            stream.Write((byte)Chat.Title);
            stream.WriteString(Chat.Message);
        }
    }
}