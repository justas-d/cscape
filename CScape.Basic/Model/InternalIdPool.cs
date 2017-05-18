using System;
using System.Collections.Generic;
using CScape.Core.Injection;

namespace CScape.Basic.Model
{
    public sealed class IdPool : IIdPool
    {
        private readonly InternalIdPool _entity = new InternalIdPool();
        private readonly InternalIdPool _player = new InternalIdPool();
        private readonly InternalIdPool _npc = new InternalIdPool();

        public uint NextEntity() => _entity.NextId();
        public void FreeEntity(uint id) => _entity.FreeId(id);

        public short NextPlayer() => Convert.ToInt16(_player.NextId() + 1);
        public void FreePlayer(short id) => _player.FreeId(Convert.ToUInt32(id - 1));

        public short NextNpc() => Convert.ToInt16(_npc.NextId() + 1);
        public void FreeNpc(short id) => _npc.FreeId(Convert.ToUInt32(id - 1));
    }

    // todo : catch OutOfIdException
    public sealed class InternalIdPool
    {
        private uint _next = 0;
        private readonly uint _limit;

        private readonly HashSet<uint> _used = new HashSet<uint>();

        public InternalIdPool(uint limit = uint.MaxValue)
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