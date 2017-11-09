using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Game.Entity;

namespace CScape.Core.Network.Entity.Component
{
    [RequiresComponent(typeof(InterfaceComponent))]
    [RequiresComponent(typeof(NetworkingComponent))]
    public sealed class InterfaceNetworkSyncComponent : EntityComponent
    {
        public override int Priority => (int)ComponentPriority.InvariantSync;

        [NotNull]
        private readonly List<IPacket> _packetQueue = new List<IPacket>();

        public InterfaceNetworkSyncComponent([NotNull] IEntity parent) : base(parent)
        {
        }

        private void Sync()
        {
            var net = Parent.AssertGetNetwork();

            if (_packetQueue.Any())
            {
                // sync queue
                foreach (var packet in _packetQueue)
                    net.SendPacket(packet);

                _packetQueue.Clear();
            }
        }

        private void InterfaceUpdate(InterfaceMessage meta)
        {
            if(meta.Packets != null)
                _packetQueue.AddRange(meta.Packets);
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case (int)MessageId.NetworkPrepare:
                    Sync();
                    break;
                case (int)MessageId.InterfaceClosed:
                {
                    InterfaceUpdate(msg.AsInterfaceClosed());
                    break;
                }
                case (int)MessageId.NewInterfaceShown:
                {
                    InterfaceUpdate(msg.AsNewInterfaceShown());
                    break;
                }
                case (int)MessageId.InterfaceUpdate:
                {
                    InterfaceUpdate(msg.AsInterfaceUpdate());
                    break;
                }
            }
        }
    }
}
