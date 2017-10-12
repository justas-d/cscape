using System;
using CScape.Core.Game.Entities;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Entities.Interface;

namespace CScape.Core.Network.Entity.Component
{
    [RequiresComponent(typeof(InterfaceComponent))]
    [RequiresComponent(typeof(NetworkingComponent))]
    public sealed class InterfaceNetworkSyncComponent : EntityComponent
    {
        public override int Priority { get; }

        [NotNull]
        private readonly List<IPacket> _packetQueue = new List<IPacket>();

        [NotNull]
        private InterfaceComponent Interfaces => Parent.Components.AssertGet<InterfaceComponent>();

        [NotNull]
        private NetworkingComponent Network => Parent.Components.AssertGet<NetworkingComponent>();

        public InterfaceNetworkSyncComponent([NotNull] Game.Entities.Entity parent) : base(parent)
        {
        }

        private void Sync()
        {
            if (_packetQueue.Any())
            {
                // sync queue
                foreach (var packet in _packetQueue)
                    Network.SendPacket(packet);

                _packetQueue.Clear();
            }

            // sync interfaces
            foreach (var meta in Interfaces.All.Values)
            {
                foreach(var packet in meta.Interface.GetUpdatePackets())
                    Network.SendPacket(packet);
            }
        }

        private void InterfaceShown(InterfaceMetadata meta)
        {
            _packetQueue.Add(meta.Interface.GetShowPacket());
        }

        private void InterfaceClosed(InterfaceMetadata meta)
        {
            _packetQueue.Add(meta.Interface.GetClosePacket());
        }

        public override void ReceiveMessage(EntityMessage msg)
        {
            switch (msg.Event)
            {
                case EntityMessage.EventType.NetworkUpdate:
                    Sync();
                    break;
                case EntityMessage.EventType.InterfaceClosed:
                {
                    InterfaceClosed(msg.AsInterfaceClosed());
                    break;
                }
                case EntityMessage.EventType.NewInterfaceShown:
                {
                    InterfaceShown(msg.AsNewInterfaceShown());
                    break;
                }
            }
        }
    }
}
