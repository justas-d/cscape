using System;
using CScape.Models.Data;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Message
{
    public sealed class PacketMessage : IGameMessage
    {
        public enum ParseStatus
        {
            Success,
            UndefinedPacket
        }

        public byte Opcode { get; }

        [NotNull]
        public Blob Data { get; }

        public ParseStatus Status { get; }

        private PacketMessage(byte opcode, [NotNull] Blob data,ParseStatus status)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Opcode = opcode;
            Status = status;
        }

        public static PacketMessage Success(byte opcode, Blob data)
            => new PacketMessage(opcode, data, ParseStatus.Success);

        public static PacketMessage Undefined(byte opcode)
            => new PacketMessage(opcode, Blob.Empty, ParseStatus.UndefinedPacket);

        public int EventId => (int)MessageId.NewPacket;
    }
}