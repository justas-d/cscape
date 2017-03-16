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
            return (short)((ReadByte() << 8) + ReadByte());
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
            Write((byte)(val >> 8));
            Write((byte)val);
        }

        public void Write32(int val)
        {
            Write((byte)(val >> 24));
            Write((byte)(val >> 16));
            Write16((short)val);
        }

        public void Write64(long val)
        {
            Write((byte)(val >> 48));
            Write((byte)(val >> 40));
            Write((byte)(val >> 32));
            Write32((int)val);
        }

        public void Overwrite(byte[] newbuffer)
        {
            Buffer = newbuffer;
            ResetWrite();
            ResetRead();
        }
    }
}
