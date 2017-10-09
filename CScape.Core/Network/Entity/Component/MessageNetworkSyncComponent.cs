using System.Diagnostics;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Network.Packet;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Component
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

        public MessageNetworkSyncComponent(Game.Entities.Entity parent)
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