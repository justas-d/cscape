using System.Diagnostics;
using CScape.Core.Game.Entities.Fragment.Component;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Injection;
using CScape.Core.Network.Packet;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Fragment.Network
{
    public sealed class MessageSyncNetFragment : IEntityNetFragment
    {
        public Entity Parent { get; }
        public int Priority { get; }

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

        public MessageSyncNetFragment(Entity parent)
        {
            Parent = parent;
        }

        public void ReceiveMessage(EntityMessage msg)
        {
            if (msg.Event == EntityMessage.EventType.NewSystemMessage)
            {
                var msgStr = msg.AsNewSystemMessage();
                Network.SendPacket(new SystemChatMessagePacket(msgStr));
            }
        }

        public void Update(IMainLoop loop, NetworkingComponent network)
        {
            
        }
    }
}