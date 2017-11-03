using System.Diagnostics;
using CScape.Core.Extensions;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Network.Packet;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Message;

namespace CScape.Core.Network.Entity.Component
{
    [RequiresComponent(typeof(NetworkingComponent))]
    public sealed class MessageNetworkSyncComponent : EntityComponent
    {
        public override int Priority => (int)ComponentPriority.MessageSync;

        private readonly SystemMessageFilter _filter = new SystemMessageFilter();

        public MessageNetworkSyncComponent(IEntity parent)
            :base(parent)
        {

            _filter.Filter((ulong)CoreSystemMessageFlags.Network);
            _filter.Filter((ulong)CoreSystemMessageFlags.Debug);
        }

        private bool CanSync()
        {
            // don't sync messages if the user has got a chat interface open.
            var interf = Parent.GetInterfaces();
            return interf?.Chat == null;
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            if (msg.EventId == (int)MessageId.NewSystemMessage)
            {
                if (!CanSync())
                    return;

                var sysMsgData = msg.AsSystemMessage();

                if (!_filter.IsFiltered(sysMsgData))
                {
                    var net = Parent.AssertGetNetwork();
                    net.SendPacket(new SystemChatMessagePacket(sysMsgData.Msg));
                }
            }
        }
    }
}