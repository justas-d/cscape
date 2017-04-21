using System;
using System.Runtime.CompilerServices;

namespace cscape
{
    internal class CircularBlobException : Exception
    {
        public CircularBlobException(string message) : base(message)
        {
        }
    }

    public class Blob
    {
        public byte[] Buffer { get; private set; }

        private int _readHead = -1;
        private int _writeHead = -1;

        public int BytesRead => _readHead + 1;
        public int BytesWritten => _writeHead + 1;

        private bool _isInBitMode = false;

        private int _readBitPos = 0;
        private int _writeBitPos = 0;

        public bool IsCircular { get; }
        private readonly byte[] _queuedForReadMask = null;

        public Blob(int size, bool isCircular = false) : this(new byte[size], isCircular)
        {
        }

        public Blob(byte[] buffer, bool isCircular = false)
        {
            Buffer = buffer;
            IsCircular = isCircular;

            if (isCircular)
            {
                var len = buffer.Length / 8;
                if (buffer.Length % 8 > 0)
                    len += 1;
                _queuedForReadMask = new byte[len];
            }
        }

        #region Byte I/O

        public void WriteBlock(byte[] src, int srcOffset, int count)
        {
            System.Buffer.BlockCopy(src, srcOffset, Buffer, _writeHead + 1, count);
            _writeHead += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetWrite() => _writeHead = -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetRead() => _readHead = -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            if (IsCircular)
            {
                var head = ++_readHead % Buffer.Length;

                var maskIndex = head / 8;
                var bitIndex = head % 8;

                // if the byte we want to read is not masked for reading, throw
                if (((_queuedForReadMask[maskIndex] >> bitIndex) & 1) == 0)
                    throw new CircularBlobException($"Attempted to read byte that was not masked for reading. MaskIndex: {maskIndex} BitIndex: {bitIndex} BufferSize: {Buffer.Length} MaskSize: {_queuedForReadMask.Length} readHead: {_readHead}");

                // unset flag (num & mask)
                _queuedForReadMask[maskIndex] &= (byte)~(1 << bitIndex);

                return Buffer[head];

            }
            return Buffer[++_readHead];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadSkipByte(int numberOfBytesToSkip = 1) => _readHead = _readHead + numberOfBytesToSkip;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16()
        {
            return (short) ((ReadByte() << 8) + ReadByte());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32()
        {
            return (ReadByte() << 24) + (ReadByte() << 16) + ReadInt16();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64()
        {
            return (ReadInt32() << 32) + ReadInt32();
        }

        public void Write(byte val)
        {
            if (IsCircular)
            {
                var head = ++_writeHead % Buffer.Length;

                var maskIndex = head / 8;
                var bitIndex = head % 8;

                // check if readable flag is set
                if (((_queuedForReadMask[maskIndex] >> bitIndex) & 1) == 1)
                    throw new CircularBlobException($"Attempted to set a byte readable but it's already readable. MaskIndex: {maskIndex} BitIndex: {bitIndex} BufferSize: {Buffer.Length} MaskSize: {_queuedForReadMask.Length} writeHead: {_writeHead}");

                // set readable flag
                _queuedForReadMask[maskIndex] |= (byte) (1 << bitIndex);

                Buffer[head] = val;
                return;
            }

            Buffer[++_writeHead] = val;
        }

        public void Write16(short val)
        {
            Write((byte) (val >> 8));
            Write((byte) val);
        }

        public void Write32(int val)
        {
            Write((byte) (val >> 24));
            Write((byte) (val >> 16));
            Write16((short) val);
        }

        public void Write64(long val)
        {
            Write((byte) (val >> 48));
            Write((byte) (val >> 40));
            Write((byte) (val >> 32));
            Write32((int) val);
        }

        #endregion

        #region Bit I/O

        // todo : support circular buffers for bit i/o

        /// <exception cref="InvalidOperationException">Already in bit mode.</exception>
        public void BeginBitAccess()
        {
            if (_isInBitMode) throw new InvalidOperationException("Already in bit access mode.");
            _isInBitMode = true;

            _readBitPos = BytesRead * 8;
            _writeBitPos = BytesWritten * 8;
        }

        public int ReadBits(int i)
        {
            var k = _readBitPos >> 3;
            var l = 8 - (_readBitPos & 7);
            var i1 = 0;
            _readBitPos += i;
            for (; i > l; l = 8)
            {
                i1 += (Buffer[k++] & Constant.MaskForBit[l]) << i - l;
                i -= l;
            }
            if (i == l) i1 += Buffer[k] & Constant.MaskForBit[l];
            else i1 += Buffer[k] >> l - i & Constant.MaskForBit[i];
            return i1;
        }

        /// <exception cref="InvalidOperationException">Not in bit mode.</exception>
        public void EndBitAccess()
        {
            if (!_isInBitMode) throw new InvalidOperationException("Not in bit access mode.");
            _isInBitMode = false;

            _readHead = ((_readBitPos + 7) / 8) - 1;
            _writeHead = ((_writeBitPos + 7) / 8) - 1;
        }

        public void WriteBits(int numBits, int value)
        {
            int bytePos = _writeBitPos >> 3;
            int bitOffset = 8 - (_writeBitPos & 7);
            _writeBitPos += numBits;

            for (; numBits > bitOffset; bitOffset = 8)
            {
                Buffer[bytePos] &= (byte) ~Constant.MaskForBit[bitOffset];
                Buffer[bytePos++] |= (byte) ((value >> (numBits - bitOffset)) & Constant.MaskForBit[bitOffset]);

                numBits -= bitOffset;
            }
            if (numBits == bitOffset)
            {
                Buffer[bytePos] &= (byte) ~Constant.MaskForBit[bitOffset];
                Buffer[bytePos] |= (byte) (value & Constant.MaskForBit[bitOffset]);
            }
            else
            {
                Buffer[bytePos] &= (byte) ~(Constant.MaskForBit[numBits] << (bitOffset - numBits));
                Buffer[bytePos] |= (byte) ((value & Constant.MaskForBit[numBits]) << (bitOffset - numBits));
            }
        }

        #endregion

        public void Overwrite(byte[] newbuffer)
        {
            Buffer = newbuffer;
            ResetWrite();
            ResetRead();
        }
    }
}
 