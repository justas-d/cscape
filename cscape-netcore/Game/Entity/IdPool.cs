using System;
using System.Collections.Generic;

namespace CScape.Game.Entity
{
    // todo : catch OutOfIdException
    public class OutOfIdException : Exception
    {
        
    }

    public sealed class IdPool
    {
        private uint _next = 0;
        private readonly uint _limit;

        private readonly HashSet<uint> _used = new HashSet<uint>();

        public IdPool(uint limit = uint.MaxValue)
        {
            _limit = limit;
        }

        public uint NextId()
        {
            unchecked
            {
                if(_used.Count >= _limit)
                    throw new OutOfIdException();

                if (_next == _limit)
                    _next = 0;

                // skip used id's.
                while(_used.Contains(_next))
                {
                    if (++_next >= _limit)
                        _next = 0;
                } 

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