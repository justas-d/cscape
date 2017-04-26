using System;
using System.Runtime.CompilerServices;

namespace CScape.Data
{
    internal class CircularBlobException : Exception
    {
        public CircularBlobException(string message) : base(message)
        {
        }
    }

    public class CircularBlob
    {
        public byte[] Buffer { get; private set; }

        public int ReadCaret { get; private set; }
        public int WriteCaret { get; private set; }

        private readonly byte[] _queuedForReadMask = null;

        public CircularBlob(int size)
        {
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));

            Buffer = new byte[size];

            var len = Buffer.Length / 8;
            if (Buffer.Length % 8 > 0)
                len += 1;
            _queuedForReadMask = new byte[len];
        }

        public void WriteBlock(byte[] src, int srcOffset, int count)
        {
            for (var i = 0; i < count; i++)
                Write(src[i]);
        }

        public void Write(byte val)
        {
            var head = WriteCaret++ % Buffer.Length;

            var maskIndex = head / 8;
            var bitIndex = head % 8;

            // check if readable flag is set
            if (((_queuedForReadMask[maskIndex] >> bitIndex) & 1) == 1)
                throw new CircularBlobException(
                    $"Attempted to set a byte readable but it's already readable. MaskIndex: {maskIndex} BitIndex: {bitIndex} BufferSize: {Buffer.Length} MaskSize: {_queuedForReadMask.Length} WriteCaret: {WriteCaret}");

            // set readable flag
            _queuedForReadMask[maskIndex] |= (byte) (1 << bitIndex);

            Buffer[head] = val;
        }

        public void ReadBlock(byte[] dest, int destOffset, int count)
        {
            for (var i = 0; i < count; i++)
                dest[i + destOffset] = ReadByte();
        }

        public byte Peek(int lookahead = 0)
        {
            var head = ReadCaret + lookahead;

            head %= Buffer.Length; // wraparound

            var maskIndex = head / 8;
            var bitIndex = head % 8;

            // if the byte we want to read is not masked for reading, throw
            if (!CanReadCircular(maskIndex, bitIndex))
                throw new CircularBlobException(
                    $"Attempted to read byte that was not masked for reading. MaskIndex: {maskIndex} BitIndex: {bitIndex} BufferSize: {Buffer.Length} MaskSize: {_queuedForReadMask.Length} ReadCaret: {ReadCaret}");

            return Buffer[head];
        }

        public byte ReadByte()
        {
            var data = Peek();
            // unset flag (num & mask)
            _queuedForReadMask[ReadCaret / 8] &= (byte) ~(1 << ReadCaret % 8);

            ++ReadCaret;
            return data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CanReadCircular(int maskIndex, int bitIndex)
            => ((_queuedForReadMask[maskIndex] >> bitIndex) & 1) == 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanRead(int lookahead = 0)
        {
            var head = (ReadCaret + lookahead) % Buffer.Length;
            return CanReadCircular(head / 8, head % 8);
        }

        public short ReadInt16()
        {
            return (short)((ReadByte() << 8) + ReadByte());
        }
    }
}