using System.Collections.Generic;
using CScape.Core.Data;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    public class PacketMetadata
    {
        public enum ParseStatus
        {
            Success,
            UndefinedPacket
        }

        public byte Opcode { get; }

        public Blob Data { get; }

        public ParseStatus Status { get; }

        private PacketMetadata(byte opcode, Blob data,ParseStatus status)
        {
            Data = data;
            Opcode = opcode;
            Status = status;
        }

        public static PacketMetadata Success(byte opcode, Blob data)
            => new PacketMetadata(opcode, data, ParseStatus.Success);

        public static PacketMetadata Undefined(byte opcode)
            => new PacketMetadata(opcode, null, ParseStatus.UndefinedPacket);
    }

    public interface IPacketParser
    {
        IEnumerable<PacketMetadata> Parse([NotNull] CircularBlob stream);
    }
}