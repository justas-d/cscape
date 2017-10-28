using System;
using System.Collections.Generic;
using System.Net.Sockets;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Network.Packet;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Component
{
    /// <summary>
    /// Responsible for managing the network part of the entity, sending out
    /// new packet and network reinitialize events.
    /// </summary>
    [RequiresComponent(typeof(PlayerComponent))]
    public sealed class NetworkingComponent : EntityComponent
    {
        [NotNull]
        private IPacketParser PacketParser { get; }

        [NotNull]
        private ISocketContext Socket { get; }

        public override int Priority => (int)ComponentPriority.Networking;

        private readonly List<IPacket> _queuedPackets = new List<IPacket>();

        /// <summary>
        /// In milliseconds, the delay between a socket dying and its entity being removed
        /// from the world.
        /// </summary>
#if DEBUG
        public long ReapTimeMs { get; set; } = 1000/* * 60*/;   
#endif

        public NetworkingComponent(
            [NotNull] IEntity parent, 
            [NotNull] ISocketContext socket, 
            [NotNull] IPacketParser packetParser)
            : base(parent)
        {
            PacketParser = packetParser ?? throw new ArgumentNullException(nameof(packetParser));
            Socket = socket ?? throw new ArgumentNullException(nameof(socket));
        }

        private void FlushPackets()
        {
            // don't do anything if there's no connection
            if (!Socket.IsConnected())
                return;

            foreach (var packet in _queuedPackets)
                packet.Send(Socket.OutStream);

            _queuedPackets.Clear();

            // send our data
            Socket.FlushOutputStream();
        }

        private void FrameUpdate()
        {
            // check if we have had a hard disconnect and if the dead time warrants a reap
            if (Socket.DeadForMs >= ReapTimeMs)
            {
                Log.Debug(this, $"Reaping {Parent}");
                Parent.Handle.System.Destroy(Parent.Handle);
                return;
            }

            // update the socket
            if (Socket.Update(Loop.GetDeltaTime()))
            {
                foreach (var packet in PacketParser.Parse(Socket.InStream))
                {
                    if(packet.Status == PacketMessage.ParseStatus.UndefinedPacket)
                        DropConnection();

                    Parent.SendMessage(packet);
                }
            }
        }

        public void DropConnection()
        {
            if (Socket.IsConnected())
            {
                Log.Debug(this, $"Dropping connection for entity {Parent}");

                SendPacket(LogoffPacket.Static);
                
                Socket.FlushOutputStream();
                Socket.Dispose();
            }
        }

        public bool CanReinitialize(int signlink) => Socket.CanReinitialize(signlink);

        public bool TryReinitializeUsing([NotNull] Socket socket, int signlink)
        {
            if (!Socket.TryReinitialize(socket, signlink))
                return false;
            
            // reinitialize was successful

            Parent.SendMessage(NotificationMessage.NetworkReinitialize);

            return true;
        }

        public void SendPacket(IPacket packet) => _queuedPackets.Add(packet);

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case SysMessage.DestroyEntity:
                {
                    DropConnection();
                    break;
                }
                case SysMessage.FrameUpdate:
                {
                    FrameUpdate();
                    break;
                }
                case SysMessage.FrameEnd:
                {
                    FlushPackets();
                    break;
                }
            }
        }
    }
}
 