using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace CScape.Core.Data
{
    public class Blob
    {
        public byte[] Buffer { get; private set; }

        // or just "Bytes(Read/Written)"
        public int ReadCaret { get; set; }
 
        public int WriteCaret { get; set; }

        private bool _isInBitMode = false;

        public int BitReadCaret { get; set; }
        public int BitWriteCaret { get; set; }

        public Blob(int size) : this(new byte[size])
        {
        }

        public Blob(byte[] buffer)
        {
            Buffer = buffer;
        }

        #region Byte I/O

        public void WriteBlock(byte[] src, int srcOffset, int count)
        {
            System.Buffer.BlockCopy(src, srcOffset, Buffer, WriteCaret, count);
            WriteCaret += count;
        }

        public void ReadBlock(byte[] dest, int destOffset, int count)
        {
            System.Buffer.BlockCopy(Buffer, ReadCaret, dest, destOffset, count);
            ReadCaret += count;
        }

        /// <summary>
        /// Flushes everything under the write caret into the given blob, past it's write head.
        /// </summary>
        /// <param name="blob"></param>
        public void FlushInto(Blob blob)
        {
            if (WriteCaret + blob.WriteCaret >= blob.Buffer.Length)
                throw new ArgumentOutOfRangeException();

            blob.WriteBlock(Buffer, 0, WriteCaret);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetWrite() => WriteCaret = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetRead() => ReadCaret = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetHeads()
        {
            ResetWrite();
            ResetRead();
        }

        public byte Peek(int lookahead = 0)
            => Buffer[ReadCaret + lookahead];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            var data = Peek();
            ++ReadCaret;
            return data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanRead(int lookahead = 0)
            => ReadCaret + lookahead >= Buffer.Length;

        public short ReadInt16()
        {
            return (short) ((ReadByte() << 8) + ReadByte());
        }

        public int ReadInt32()
        {
            return (ReadByte() << 24) + (ReadByte() << 16) + ReadInt16();
        }

        public long ReadInt64()
        {
            return (ReadInt32() << 32) + ReadInt32();
        }

        public const int MaxStringLength = 255;

        public bool TryReadString(out string rsString, int maxLength = MaxStringLength)
        {
            var builder = new StringBuilder(maxLength);
            var retval = true;

            try
            {
                var i = 0;
                for (; i <= maxLength; i++) // <= due to terminator char
                {
                    var c = ReadByte();
                    if (c == Constant.StringNullTerminator)
                        break;

                    builder.Append(Convert.ToChar(c));
                }
                if (i > maxLength)
                {
                    // null terminator not found within [0; maxLength], return.
                    rsString = null;
                    return false;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                retval = false;
            }
            rsString = builder.ToString();
            return retval;
        }

        public void WriteString(string str)
        {
            foreach (var c in str)
                Write((byte)c);
            Write(Constant.StringNullTerminator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte val)
            => Buffer[WriteCaret++] = val;

        public void Write16(short val)
        {
            Write((byte) (val >> 8));
            Write((byte) val);
        }

        public void Write32(int val)
        {
            Write((byte) (val >> 24));
            Write((byte) (val >> 16));
            Write((byte)(val >> 8));
            Write((byte)val);
        }

        public void Write64(long val)
        {
            Write32((int)(val >> 32));
            Write32((int)val);
        }

        #endregion

        #region Bit I/O

        /// <exception cref="InvalidOperationException">Already in bit mode.</exception>
        public void BeginBitAccess()
        {
            if (_isInBitMode) throw new InvalidOperationException("Already in bit access mode.");
            _isInBitMode = true;

            BitReadCaret = ReadCaret * 8;
            BitWriteCaret = WriteCaret * 8;
        }

        public int ReadBits(int i)
        {
            var k = BitReadCaret >> 3;
            var l = 8 - (BitReadCaret & 7);
            var i1 = 0;
            BitReadCaret += i;
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

            ReadCaret = ((BitReadCaret + 7) / 8);
            WriteCaret= ((BitWriteCaret + 7) / 8);
        }

        public void WriteBits(int numBits, int value)
        {
            int bytePos = BitWriteCaret >> 3;
            int bitOffset = 8 - (BitWriteCaret & 7);
            BitWriteCaret += numBits;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Overwrite(byte[] newbuffer, int readhead, int writehead)
        {
            Buffer = newbuffer;
            ReadCaret = readhead;
            WriteCaret = writehead;
        }
    }
}
 