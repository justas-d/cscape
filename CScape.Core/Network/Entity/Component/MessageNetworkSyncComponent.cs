using System.Diagnostics;
using CScape.Core.Extensions;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Network.Packet;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Component
{
    [RequiresComponent(typeof(NetworkingComponent))]
    public sealed class MessageNetworkSyncComponent : EntityComponent
    {
        // TODO : MessageNetworkSyncComponent priority
        public override int Priority { get; }

        public bool SyncDebugMessages { get; }

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

        public MessageNetworkSyncComponent(IEntity parent)
            :base(parent)
        {
#if DEBUG
            SyncDebugMessages = true;
#endif
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            if (msg.EventId == (int)MessageId.NewSystemMessage)
            {
                var msgStr = msg.AsSystemMessage();
                var isDebugBitSet = (msgStr.Flags & (ulong)CoreSystemMessageFlags.Debug) != 0;

                if (!isDebugBitSet || SyncDebugMessages)
                    Network.SendPacket(new SystemChatMessagePacket(msgStr.Msg));
            }
        }

    }
}