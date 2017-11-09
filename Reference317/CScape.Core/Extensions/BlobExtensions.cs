using System.Diagnostics;
using CScape.Models.Data;

namespace CScape.Core.Extensions
{
    public static class BlobExtensions
    {
        [DebuggerStepThrough]
        [DebuggerHidden]
        public static BlobPlaceholder Placeholder(this Blob blob, int size)
        {
            return new BlobPlaceholder(blob, blob.WriteCaret, size);
        }
    }
}
