using System;
using System.Diagnostics;
using CScape.Core.Extensions;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Component
{
    public sealed class PacketDispatcherComponent: EntityComponent
    {
        private readonly IPacketHandlerCatalogue _handlers;

        public override int Priority => (int) ComponentPriority.PacketDispatcher;

        public bool ShouldNotifyAboutUnhandledPackets { get; set; } = true;
        public bool ShouldNotifyAboutPacketsBeingHandled { get; set; } = true;

        public PacketDispatcherComponent(IEntity parent, [NotNull] IPacketHandlerCatalogue handlers)
            :base(parent)
        {
            _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
        }

        private void HandlePacket(PacketMessage packet)
        {
            switch (packet.Status)
            {
                case PacketMessage.ParseStatus.Success:
                {
                    var handler = _handlers.GetHandler(packet.Opcode);

                    if (handler != null)
                    {
                        handler.Handle(Parent, packet);

                        if(ShouldNotifyAboutPacketsBeingHandled)
                            Parent.SystemMessage($"Packet: {packet.Opcode:000} -> {handler.GetType().Name}", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Network);
                    }
                    else if(ShouldNotifyAboutUnhandledPackets)
                        Parent.SystemMessage($"Unhandled packet opcode: {packet.Opcode:000}", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Network);
                    
                    break;
                }
                case PacketMessage.ParseStatus.UndefinedPacket:
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

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case (int)MessageId.NewPacket:
                {
                    HandlePacket(msg.AsNewPacket());
                    break;
                }
            }
           
        }
    }
}