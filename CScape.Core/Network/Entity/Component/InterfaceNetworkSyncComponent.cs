using CScape.Core.Game.Entities;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using CScape.Core.Extensions;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Entities.Message;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Message;

namespace CScape.Core.Network.Entity.Component
{
    [RequiresComponent(typeof(InterfaceComponent))]
    [RequiresComponent(typeof(NetworkingComponent))]
    public sealed class InterfaceNetworkSyncComponent : EntityComponent
    {
        public override int Priority { get; }

        [NotNull]
        private readonly List<IPacket> _packetQueue = new List<IPacket>();

        public InterfaceNetworkSyncComponent([NotNull] Game.Entities.Entity parent) : base(parent)
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
                case (int)MessageId.NetworkUpdate:
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
