using System;
using System.Collections.Generic;

namespace CScape.Game.Entity
{
    public sealed class IdPool
    {
        private uint _next = 0;

        private readonly HashSet<uint> _used = new HashSet<uint>();

        public uint NextId()
        {
            unchecked
            {
                // skip used id's.
                while (_used.Contains(_next++)) { }

                _used.Add(_next);
                return _next;
            }
        }

        public void FreeId(uint id)
        {
            if (!_used.Contains(id))
                throw new InvalidOperationException($"Tried to free unused id {id}");

            _used.Remove(id);
        }
    }
}