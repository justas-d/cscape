using System;

namespace CScape.Core.Game.Entities.Message
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
        Entity
    }


    public sealed class SystemMessage
    {
        public string Msg { get; }
        public SystemMessageFlags Flags { get; }

        public SystemMessage(string msg, SystemMessageFlags flags = SystemMessageFlags.None)
        {
            Msg = msg;
        }
    }
}