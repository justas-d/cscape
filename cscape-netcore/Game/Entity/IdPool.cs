using System.Collections.Generic;

namespace CScape.Game.Entity
{
    public sealed class IdPool
    {
        private uint _next = 0;
        private readonly Queue<uint> _recycle = new Queue<uint>();

        public uint NextId()
        {
            if (_recycle.Count > 0)
                return _recycle.Dequeue();

            return _next++;
        }

        public void FreeId(uint id)
        {
            _recycle.Enqueue(id);
        }
    }
}