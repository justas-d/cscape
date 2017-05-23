using System;
using System.Runtime.CompilerServices;

namespace CScape.Core.Data
{
    public struct BlobPlaceholder
    {
        public Blob Blob { get; }
        public int StartPosition { get; }
        public int Size { get; }

        private int _origWritePos;

        public BlobPlaceholder(Blob blob, int startPosition, int size)
        {
            Blob = blob;
            StartPosition = startPosition;
            Size = size;
            _origWritePos = 0;

            // write placeholder 0's
            for (var i = 0; i < size; i++)
                Blob.Write(0);
        }

        /// <summary>
        /// Jumps to the start index and caches current write idx
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Reverse()
        {
            _origWritePos = Blob.WriteCaret;
            Blob.WriteCaret = StartPosition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfOutOfRange()
        {
            if (Blob.WriteCaret - StartPosition > Size)
                throw new InvalidOperationException("Placeholder went out of range.");
        }

        /// <summary>
        /// Resets the write caret to its original write pos.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Forward() => Blob.WriteCaret = _origWritePos;

        public void Write(Action<Blob> write)
        {
            Reverse();

            write(Blob);
            ThrowIfOutOfRange();

            Forward();
        }

        public void WriteSize()
        {
            Reverse();

            var size = _origWritePos - StartPosition - Size;

            if (size <= byte.MaxValue)
                Blob.Write((byte)size);

            else if (size <= short.MaxValue)
                Blob.Write16((short) size);

            else
                Blob.Write32(size);

            ThrowIfOutOfRange();
            Forward();
        }
    }
}