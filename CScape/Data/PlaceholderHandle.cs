using System;
using JetBrains.Annotations;

namespace CScape.Data
{
    public struct PlaceholderHandle
    {
        public Blob OriginalBlob { get; }
        public int StartIndex { get; }
        public int Size { get; }

        private int _origWritePos;

        public PlaceholderHandle([NotNull] Blob blob, int size) : this(blob, blob.WriteCaret, size)
        {
            
        }

        public PlaceholderHandle([NotNull] Blob origBlob, int startIndex, int size)
        {
            _origWritePos = -1;
            OriginalBlob = origBlob;
            StartIndex = startIndex;
            Size = size;
            if (origBlob == null) throw new ArgumentNullException(nameof(origBlob));
            if (startIndex < 0 || startIndex >= origBlob.Buffer.Length) throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (size <= 0 || startIndex + size >= origBlob.Buffer.Length) throw new ArgumentOutOfRangeException(nameof(size));
            
            // write placeholder 0's
            for(var i = 0; i < size; i++)
                origBlob.Write(0);
        }

        /// <summary>
        /// Jumps to the start index and caches current write idx
        /// </summary>
        private void Reverse()
        {
            _origWritePos = OriginalBlob.WriteCaret;
            OriginalBlob.WriteCaret = StartIndex;
        }

        /// <summary>
        /// Resets the write caret to its original write pos.
        /// </summary>
        private void Forward()
        {
            OriginalBlob.WriteCaret = _origWritePos;
        }

        public void DoWrite(Action<Blob> expr)
        {
            // set caret to startindex
            Reverse();
            expr(OriginalBlob);

            // check if we've gone out of range. (size)
            if(OriginalBlob.WriteCaret - StartIndex > Size)
                throw new InvalidOperationException("Placeholder went out of range.");

            Forward();

            // reset caret
        }

        public void WriteSize()
        {
            var written = OriginalBlob.WriteCaret - StartIndex - Size;
            Reverse();

            switch (Size)
            {
                case sizeof(byte):
                    OriginalBlob.Write((byte) written);
                    break;
                case sizeof(short):
                    OriginalBlob.Write16((short)written);
                    break;
                default:
                    throw new InvalidOperationException("Tried to write size for placeholder with unsupported Size param.");
            }

            Forward();
        }
    }
}