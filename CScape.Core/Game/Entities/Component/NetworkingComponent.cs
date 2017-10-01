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
    [RequiresFragment(typeof(PlayerComponent))]
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

        public void Update(IMainLoop loop)
        {
            // check if we have had a hard disconnect and if the dead time warrants a reap
            if (Socket.DeadForMs >= ReapTimeMs)
            {
                _log.Debug(this, $"Reaping {Parent}");
                Parent.Handle.System.Destroy(Parent.Handle);
                return;
            }

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
                            break;
                        }

                        case PacketMetadata.ParseStatus.UndefinedPacket:
                        {
                            Parent.SendMessage(
                                new EntityMessage(
                                    this,
                                    EntityMessage.EventType.UndefinedPacket, 
                                    packet));

                            DropConnection();
                            break;
                        }

                        default: throw new ArgumentOutOfRangeException();

                    }

                }
            }
        }

        public void DropConnection()
        {
            if (Socket.IsConnected())
            {
                _log.Debug(this, $"Dropping connection for entity {Parent}");

                Socket.FlushOutputStream();
                Socket.Dispose();
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
            switch (msg.Event)
            {
                case EntityMessage.EventType.DestroyEntity:
                {
                    DropConnection();
                    break;
                }
            }
        }
    }
}
 