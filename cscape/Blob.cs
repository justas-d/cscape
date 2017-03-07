using System.Runtime.CompilerServices;

namespace cscape
{
    public class Blob
    {
        public byte[] Buffer { get; private set; }
        public int ReadHead { get; set; } = -1;
        public int WriteHead { get; set; } = -1;

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
            System.Buffer.BlockCopy(src, offset, Buffer, WriteHead+1, count);
            WriteHead += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetWrite() => WriteHead = -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetRead() => ReadHead = -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            return Buffer[++ReadHead];
        }

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
            Buffer[++WriteHead] = val;
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
            Write32((int) val);
        }

        public void Overwrite(byte[] newbuffer)
        {
            Buffer = newbuffer;
            ResetWrite();
            ResetRead();
        }
    }
}
