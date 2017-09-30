using System;
using System.Diagnostics;
using CScape.Core.Injection;
using JetBrains.Annotations;
using System.Net.Sockets;
using CScape.Core.Network;

namespace CScape.Core.Game.NewEntity
{
    /// <summary>
    /// Responsible for managing the network part of the entity.
    /// </summary>
    public sealed class NetworkingComponent : IEntityComponent
    {
        [NotNull]
        private IPacketParser PacketParser { get; }

        [NotNull]
        private IPacketDispatch PacketDispatch { get; }

        [NotNull]
        private ISocketContext Socket { get; }
        public Entity Parent { get; }

        private readonly ILogger _log;

        public NetworkingComponent(
            [NotNull] Entity parent, 
            [NotNull] ISocketContext socket,
            [NotNull] IPacketParser packetParser,
            [NotNull] IPacketDispatch packetDispatch)
        {
            PacketParser = packetParser ?? throw new ArgumentNullException(nameof(packetParser));
            PacketDispatch = packetDispatch ?? throw new ArgumentNullException(nameof(packetDispatch));
            Socket = socket ?? throw new ArgumentNullException(nameof(socket));
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _log = parent.Server.Services.ThrowOrGet<ILogger>();
        }

        public void Update(IMainLoop loop)
        {
            // update the socket
            if (Socket.Update(loop.DeltaTime + loop.ElapsedMilliseconds))
            {
                foreach (var packet in PacketParser.Parse(Socket.InStream))
                {
                    switch (packet.Status)
                    {
                        case PacketMetadata.ParseStatus.Success:
                        {
                            if (PacketDispatch.CanHandle(packet.Opcode))
                            {
                                PacketDispatch.Handle(Parent, packet);
                            }
                            else
                            {
                                Parent.SendMessage(
                                    new EntityMessage(
                                        this, EntityMessage.EventType.UnhandledPacket, packet));
                            }
                            else if (player.DebugPackets)
                            {
                                player.SendSystemChatMessage($"Unhandled packet opcode: {opcode:000}");
                            }
                            break;
                        }

                        case PacketMetadata.ParseStatus.UndefinedPacket:
                        {
                            var msg = $"Undefined packet opcode: {packet.Opcode}";
                            var player = Parent.GetComponent<PlayerComponent>();
                            _log.Warning(this, msg);
                            player.ForcedLogout();

#if DEBUG
                            Debug.Fail(msg);
#endif
                            break;
                        }

                        default: throw new ArgumentOutOfRangeException();

                    }

                }
            }
        }

        public bool TryReinitializeUsing([NotNull] Socket socket, int signlink)
        {
            if (!Socket.TryReinitialize(socket, signlink))
                return false;
            
            // reinitialize was successful

            Parent.SendMessage(
                new EntityMessage(
                    this, EntityMessage.EventType.NetworkReinitialize, null));

            return true;
        }

        public bool IsConnected() => Socket.IsConnected();

        public void SendPacket(IPacket);

        public void ReceiveMessage(EntityMessage msg)
        {
            throw new NotImplementedException();
        }
    }
}
 