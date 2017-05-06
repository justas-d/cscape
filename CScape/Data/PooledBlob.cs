using System.Diagnostics;

namespace CScape.Data
{
    internal class PooledBlob
    {
        public const int BlobSize = 0x1000;

        public readonly Blob Blob = new Blob(BlobSize);
        private readonly ObjectPool<PooledBlob> _pool;

        private PooledBlob(ObjectPool<PooledBlob> pool)
        {
            Debug.Assert(pool != null);
            _pool = pool;
        }

        public int Length => Blob.Buffer.Length;

        public void Free()
        {
            Blob.ResetHeads();
            _pool.Free(this);
        }

        // if someone needs to create a private pool;
        /// <summary>
        /// If someone need to create a private pool
        /// </summary>
        /// <param name="size">The size of the pool.</param>
        /// <returns></returns>
        public static ObjectPool<PooledBlob> CreatePool(int size = 32)
        {
            ObjectPool<PooledBlob> pool = null;
            pool = new ObjectPool<PooledBlob>(() => new PooledBlob(pool), size);
            return pool;
        }

        public static implicit operator Blob(PooledBlob obj) => obj.Blob;
    }
}
