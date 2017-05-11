using System;
using CScape.Core.Injection;

namespace CScape.Basic.Model
{
    public sealed class PlayerIdPool : IPlayerIdPool
    {
        private readonly IdPool _pool = new IdPool(Convert.ToUInt32(short.MaxValue));

        public short NextId() => Convert.ToInt16(_pool.NextId() + 1);
        public void FreeId(short id) => _pool.FreeId(Convert.ToUInt32(id + 1));
    }
}