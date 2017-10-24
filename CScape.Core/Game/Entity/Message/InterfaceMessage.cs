using System;
using System.Collections.Generic;
using CScape.Core.Network;
using CScape.Models.Game.Interface;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Message
{
    public sealed class InterfaceMessage : IGameMessage
    {
        public IGameInterface Interf { get; }
        [CanBeNull]
        public IEnumerable<IPacket> Packets { get; }
        public int EventId { get; }

        public static InterfaceMessage Show(IGameInterface interf,
            [CanBeNull] params IPacket[] packets)
            => new InterfaceMessage(interf, packets, MessageId.NewInterfaceShown);

        public static InterfaceMessage Close(IGameInterface interf,
            [CanBeNull] params IPacket[] packets)
            => new InterfaceMessage(interf, packets, MessageId.InterfaceClosed);

        public static InterfaceMessage Update(IGameInterface interf,
            [CanBeNull] params IPacket[] packets)
            => new InterfaceMessage(interf, packets, MessageId.InterfaceUpdate);

        private InterfaceMessage([NotNull] IGameInterface interf, IEnumerable<IPacket> packets, MessageId id)
        {
            Interf = interf ?? throw new ArgumentNullException(nameof(interf));
            Packets = packets;
            EventId = (int)id;
        }
    }
}
