using System;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Network.Entity.Flag;

namespace CScape.Core.Network.Entity
{
    public sealed class LocalPlayerUpdateWriter : PlayerUpdateWriter
    {
        public LocalPlayerUpdateWriter(FlagAccumulatorComponent flags) : base(flags)
        {
        }

        protected override int GetHeader(Func<FlagType, int> converter)
        {
            PlayerFlag retval = 0;

            foreach (var flag in this)
            {
                // don't sync chat messages that the player sent
                if (flag.Type == FlagType.ChatMessage)
                {
                    var chat = flag as PlayerChatUpdateFlag;
                    if (chat.Chat.IsForced)
                        retval |= flag.Type.ToPlayer();
                }
                else
                {
                    retval |= flag.Type.ToPlayer();
                }
            }

            return (int)retval;
        }
    }
}