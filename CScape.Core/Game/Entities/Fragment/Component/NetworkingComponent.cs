using System;
using System.Collections.Generic;
using System.Net.Sockets;
using CScape.Core.Data;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Injection;
using CScape.Core.Network;
using CScape.Core.Network.Packet;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Fragment.Component
{
    /// <summary>
    /// Responsible for managing the network part of the entity, sending out
    /// new packet and network reinitialize events.
    /// </summary>
    [RequiresFragment(typeof(PlayerComponent))]
    public sealed class NetworkingComponent : IEntityComponent
    {
        [NotNull]
        private IPacketParser PacketParser { get; }

        [NotNull]
        private ISocketContext Socket { get; }
        public Entity Parent { get; }

        public int Priority { get; } = -1;

        private readonly List<IPacket> _queuedPackets = new List<IPacket>();

        private readonly ILogger _log;

        [NotNull]
        public OutBlob OutStream => Socket.OutStream;

        /// <summary>
        /// In milliseconds, the delay between a socket dying and it's player being removed
        /// from the world.
        /// </summary>
        public long ReapTimeMs { get; set; } = 1000 * 60;

        public NetworkingComponent(
            [NotNull] Entity parent, 
            [NotNull] ISocketContext socket, [NotNull] IPacketParser packetParser)
        {
            PacketParser = packetParser ?? throw new ArgumentNullException(nameof(packetParser));
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
                sync.Update(loop, this);

            foreach (var packet in _queuedPackets)
                packet.Send(Socket.OutStream);

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
                    if(packet.Status == PacketMetadata.ParseStatus.UndefinedPacket)
                        DropConnection();

                    Parent.SendMessage(
                        new EntityMessage(
                            this, EntityMessage.EventType.NewPacket, packet));
                }
            }
        }

        public void DropConnection()
        {
            if (Socket.IsConnected())
            {
                _log.Debug(this, $"Dropping connection for entity {Parent}");

                SendPacket(LogoffPacket.Static);
                
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

        public void SendPacket(IPacket packet) => _queuedPackets.Add(packet);

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
 