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
        public override int Priority => (int)ComponentPriority.MessageLogComponent;
        private readonly SystemMessageFilter _filter = new SystemMessageFilter();

        public MessageLogComponent(IEntity parent)
            :base(parent)
        {
            _filter.Filter((ulong) CoreSystemMessageFlags.Debug);
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            if (msg.EventId == (int)MessageId.NewSystemMessage)
            {
                var data = msg.AsSystemMessage();

                if(!_filter.IsFiltered(data))
                {
                    Log.Normal(this, $"({data.Flags}) ({Parent}): {data.Msg}");
                }
            }
        }
    }
}