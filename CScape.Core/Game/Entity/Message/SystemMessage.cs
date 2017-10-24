using System;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Message
{
    [Flags]
    public enum SystemMessageFlags
    {
        None = 0,

        // debug messages
        /// <summary>
        /// Should not be printed to a player, only print when debugging.
        /// </summary>
        Debug,
        Skill,
        Item,
        Network,
        Interface,
        Entity,
        Command
    }


    public sealed class SystemMessage : IGameMessage
    {
        [NotNull]
        public string Msg { get; }
        public SystemMessageFlags Flags { get; }

        public int EventId => (int)MessageId.NewSystemMessage;

        public SystemMessage([NotNull] string msg, SystemMessageFlags flags = SystemMessageFlags.None)
        {
            if (string.IsNullOrEmpty(msg))
            {
                throw new ArgumentException("message", nameof(msg));
            }

            Msg = msg;
        }
    }
}