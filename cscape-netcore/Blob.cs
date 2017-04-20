using System;
using System.Runtime.CompilerServices;

namespace cscape
{
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

        /// <summary>
        /// Wrapper constructor. Wraps a given buffer.
        /// </summary>
        public Blob(byte[] existingBuffer)
        {
            Buffer = existingBuffer;
        }

        /// <summary>
        /// Constructs a new buffer of size.
        /// </summary>
        public Blob(int size)
        {
            Buffer = new byte[size];
        }

        #region Byte I/O

        public void WriteBlock(byte[] src, int offset, int count)
        {
            System.Buffer.BlockCopy(src, offset, Buffer, _writeHead + 1, count);
            _writeHead += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetWrite() => _writeHead = -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetRead() => _readHead = -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {

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