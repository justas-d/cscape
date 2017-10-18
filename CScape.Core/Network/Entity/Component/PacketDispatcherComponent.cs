using System;
using System.Diagnostics;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Message;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Component
{
    public sealed class PacketDispatcherComponent: EntityComponent
    {
        private readonly IPacketHandlerCatalogue _handlers;

        public override int Priority { get; }

        public bool ShouldNotifyAboutUnhandledPackets { get; set; } = true;
        public bool ShouldNotifyAboutPacketsBeingHandled { get; set; } = true;

        public PacketDispatcherComponent(Game.Entities.Entity parent, [NotNull] IPacketHandlerCatalogue handlers)
            :base(parent)
        {
            _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
        }

        private void HandlePacket(PacketMetadata packet)
        {
            switch (packet.Status)
            {
                case PacketMetadata.ParseStatus.Success:
                {
                    var handler = _handlers.GetHandler(packet.Opcode);

                    if (handler != null)
                    {
                        handler.Handle(Parent, packet);

                        if(ShouldNotifyAboutPacketsBeingHandled)
                            Parent.SystemMessage($"Packet: {packet.Opcode:000} -> {handler.GetType().Name}", SystemMessageFlags.Debug | SystemMessageFlags.Network);
                    }
                    else if(ShouldNotifyAboutUnhandledPackets)
                        Parent.SystemMessage($"Unhandled packet opcode: {packet.Opcode:000}", SystemMessageFlags.Debug | SystemMessageFlags.Network);
                    
                    break;
                }
                case PacketMetadata.ParseStatus.UndefinedPacket:
                {
                    Parent.Log.Warning(this, $"Undefined packet opcode: {packet.Opcode}");
#if DEBUG
                    Debug.Fail($"Undefined packet opcode: {packet.Opcode}");
#endif

                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void ReceiveMessage(GameMessage msg)
        {
            switch (msg.Event)
            {
                case GameMessage.Type.NewPacket:
                {
                    HandlePacket(msg.AsNewPacket());
                    break;
                }
            }
           
        }
    }
}