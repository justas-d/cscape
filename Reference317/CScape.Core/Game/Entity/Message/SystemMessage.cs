using System;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Message
{
    [Flags]
    public enum CoreSystemMessageFlags : ulong
    {
        None = SystemMessageFlags.None,
        Normal = SystemMessageFlags.Normal,
        Debug = SystemMessageFlags.Debug,
        Skill = 2,
        Item = 4,
        Network = 8,
        Interface = 16,
        Entity = 32
    }

    public sealed class SystemMessage : IGameMessage
    {
        [NotNull]
        public string Msg { get; }
        public ulong Flags { get; }

        public int EventId => (int)MessageId.NewSystemMessage;

        public SystemMessage([NotNull] string msg, ulong flags = (ulong)SystemMessageFlags.Normal)
        {
            if (string.IsNullOrEmpty(msg))
            {
                throw new ArgumentException("message", nameof(msg));
            }

            Flags = flags;
            Msg = msg;
        }
    }
}