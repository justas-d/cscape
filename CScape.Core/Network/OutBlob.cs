using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using CScape.Core;
using CScape.Core.Network;

namespace CScape.Models.Data
{
    public class PacketSizeDoesNotMatchWrittenSizeException : Exception
    {
        public byte Opcode { get; }
        public PacketLength Size { get; }
        public int WrittenSize { get; }

        public PacketSizeDoesNotMatchWrittenSizeException(
            byte opcode, PacketLength size, int writtenSize)
        {
            Opcode = opcode;
            Size = size;
            WrittenSize = writtenSize;
        }

        public override string ToString() => $"Wrote too much ({WrittenSize}) for packet size {Size} opcode {Opcode}";
    }

    public class AttemptedToWriteUndefinedPacketException : Exception
    {
        
    }

    public class OutBlob : Blob
    {
        private struct PacketFrame
        {
            public byte Opcode { get; }
            public PacketLength Size { get; }
            public BlobPlaceholder? Placeholder { get; }
            public int AtWriteHead { get; }

            public PacketFrame(byte opcode, PacketLength size, int writeHead, BlobPlaceholder? placeholder = null)
            {
                Opcode = opcode;
                Size = size;
                Placeholder = placeholder;
                AtWriteHead = writeHead;
            }
        }

        private readonly IPacketDatabase _db;

        public OutBlob(IServiceProvider service, int size) : base(new byte[size])
        {
            _db = service.ThrowOrGet<IPacketDatabase>();
        }

        private readonly Stack<PacketFrame> _frames = new Stack<PacketFrame>();

        public void BeginPacket(byte id)
        {
            var size = _db.GetOutgoing(id);

            if (size == PacketLength.Undefined)
                throw new AttemptedToWriteUndefinedPacketException();

            // write opcode now, ph must follow
            Write(id);

            // create ph and packet frame
            switch (size)
            {
                case PacketLength.NextByte:
                    _frames.Push(new PacketFrame(id, size, WriteCaret, new BlobPlaceholder(this, WriteCaret, sizeof(byte))));
                    break;
                case PacketLength.NextShort:
                    _frames.Push(new PacketFrame(id, size, WriteCaret, new BlobPlaceholder(this, WriteCaret, sizeof(short))));
                    break;
                default:
                    _frames.Push(new PacketFrame(id, size, WriteCaret));
                    break;
            }
        }

        public void EndPacket()
        {
            var frame = _frames.Pop();
            var written = WriteCaret - frame.AtWriteHead;

            if (frame.Placeholder != null)
            {
                // write size in ph
                var ph = frame.Placeholder.Value;
                written -= ph.Size;

                ph.Write(b =>
                {
                    if (frame.Size == PacketLength.NextByte)
                        Write((byte)written);
                    else if (frame.Size == PacketLength.NextShort)
                        Write16((short)written);
                });
            }
            else
            {
                // verify that we have written up to the size given in opcode
                var size = (int) frame.Size;
                if (written > size)
                    throw new PacketSizeDoesNotMatchWrittenSizeException(frame.Opcode, frame.Size, written);
            }
        }

        public void WriteByteInt32Smart(int value)
        {
            if (value >= 255)
            {
                Write(255);
                Write32(value);
            }
            else
                Write((byte)value);
        }
    }
}