using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Message;

namespace CScape.Core.Game.Entity.Component
{
    /// <summary>
    /// Logs system messages to ILogger
    /// </summary>
    public sealed class MessageLogComponent : EntityComponent
    {
        public override int Priority { get; }


        public MessageLogComponent(IEntity parent)
            :base(parent)
        {
            
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            if (msg.EventId == (int)MessageId.NewSystemMessage)
            {
                var strMSg = msg.AsSystemMessage();
                Log.Normal(this, $"({strMSg.Flags}) ({Parent}): {strMSg.Msg}");
            }
        }
    }
}