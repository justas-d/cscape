using System.Collections.Generic;
using CScape.Core.Network;
using CScape.Models.Game.Interface;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Message
{
    public sealed class InterfaceMessage : IGameMessage
    {
        public InterfaceMetadata Interf { get; }
        [CanBeNull]
        public IEnumerable<IPacket> Packets { get; }
        public int EventId { get; }

        public static InterfaceMessage Show(InterfaceMetadata interf,
            [CanBeNull] IEnumerable<IPacket> packets)
            => new InterfaceMessage(interf, packets, MessageId.NewInterfaceShown);

        public static InterfaceMessage Close(InterfaceMetadata interf,
            [CanBeNull] IEnumerable<IPacket> packets)
            => new InterfaceMessage(interf, packets, MessageId.InterfaceClosed);

        public static InterfaceMessage Update(InterfaceMetadata interf,
            [CanBeNull] IEnumerable<IPacket> packets)
            => new InterfaceMessage(interf, packets, MessageId.InterfaceUpdate);

        private InterfaceMessage(InterfaceMetadata interf, 
            [CanBeNull] IEnumerable<IPacket> packets, int id)
        {
            Interf = interf;
            Packets = packets;
            EventId = id;
        }
    }
}
