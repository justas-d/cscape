using System;
using System.Diagnostics;
using System.Net.Sockets;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Injection;
using CScape.Core.Network;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Component
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

        public int Priority { get; } = -1;

        private readonly ILogger _log;

        /// <summary>
        /// In milliseconds, the delay between a socket dying and it's player being removed
        /// from the world.
        /// </summary>
        public long ReapTimeMs { get; set; } = 1000 * 60;

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

        public void Sync([NotNull] IMainLoop loop)
        {
            // don't do anything if there's no connection
            if (!Socket.IsConnected())
                return;

            // write our data
            foreach (var sync in Parent.Network)
                sync.Update(loop);

            // send our data
            Socket.FlushOutputStream();
        }

        private void CheckForHardDisconnect()
        {
            if (Socket.DeadForMs >= ReapTimeMs)
            {
                _log.Debug(this, $"Reaping {Parent}");
                return;
            }
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
                            var player = Parent.Components.Get<PlayerComponent>();
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

        public void SendPacket(IPacket);

        public void ReceiveMessage(EntityMessage msg)
        {
            throw new NotImplementedException();
        }
    }
}
 