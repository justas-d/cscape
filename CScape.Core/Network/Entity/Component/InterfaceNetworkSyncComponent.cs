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
            _packetQueue.AddRange(meta.Interface.GetShowPackets());
        }

        private void InterfaceClosed(InterfaceMetadata meta)
        {
            _packetQueue.AddRange(meta.Interface.GetClosePackets());
        }

        public override void ReceiveMessage(GameMessage msg)
        {
            switch (msg.Event)
            {
                case GameMessage.Type.NetworkUpdate:
                    Sync();
                    break;
                case GameMessage.Type.InterfaceClosed:
                {
                    InterfaceClosed(msg.AsInterfaceClosed());
                    break;
                }
                case GameMessage.Type.NewInterfaceShown:
                {
                    InterfaceShown(msg.AsNewInterfaceShown());
                    break;
                }
            }
        }
    }
}
