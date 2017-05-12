using System;
using System.Runtime.CompilerServices;

namespace CScape.Core.Data
{
    public struct DisposableBlobPlaceholder
    {
        private readonly Blob _blob;
        private readonly int _startPos;
        private readonly int _size;

        private int _origWritePos;

        public DisposableBlobPlaceholder(Blob blob, int startPos, int size)
        {
            _blob = blob;
            _startPos = startPos;
            _size = size;
            _origWritePos = 0;

            // write placeholder 0's
            for (var i = 0; i < size; i++)
                _blob.Write(0);
        }

        /// <summary>
        /// Jumps to the start index and caches current write idx
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Reverse()
        {
            _origWritePos = _blob.WriteCaret;
            _blob.WriteCaret = _startPos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfOutOfRange()
        {
            if (_blob.WriteCaret - _startPos > _size)
                throw new InvalidOperationException("Placeholder went out of range.");
        }

        /// <summary>
        /// Resets the write caret to its original write pos.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Forward() => _blob.WriteCaret = _origWritePos;

        public void Write(Action<Blob> write)
        {
            Reverse();

            write(_blob);
            ThrowIfOutOfRange();

            Forward();
        }

        public void WriteSize()
        {
            Reverse();

            var size = _origWritePos - _startPos - _size;

            if (size <= byte.MaxValue)
                _blob.Write((byte)size);

            else if (size <= short.MaxValue)
                _blob.Write16((short) size);

            else
                _blob.Write32(size);

            ThrowIfOutOfRange();
            Forward();
        }
    }
}