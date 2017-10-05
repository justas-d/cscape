using System.Diagnostics;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Network.Packet;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Component
{
    [RequiresComponent(typeof(NetworkingComponent))]
    public sealed class MessageNetworkSyncComponent : EntityComponent
    {
        // TODO : MessageNetworkSyncComponent priority
        public override int Priority { get; }

        [NotNull]
        private NetworkingComponent Network
        {
            get
            {
                var val = Parent.Components.Get<NetworkingComponent>();
                Debug.Assert(val != null);
                return val;
            }
        }

        public MessageNetworkSyncComponent(Entity parent)
            :base(parent)
        {
            
        }

        public override void ReceiveMessage(EntityMessage msg)
        {
            if (msg.Event == EntityMessage.EventType.NewSystemMessage)
            {
                var msgStr = msg.AsNewSystemMessage();
                Network.SendPacket(new SystemChatMessagePacket(msgStr));
            }
        }

    }
}