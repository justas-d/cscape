using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace cscape
{
    public class EntityPool<T> : IEnumerable<T> where T : Entity
    {
        public int Size { get; }

        private readonly Stack<int> _idPool;
        private readonly T[] _pool;

        public EntityPool(int size)
        {
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));

            Size = size;
            _idPool = new Stack<int>(Size);
            _pool = new T[Size];

            for (var i = Size - 1; i >= 0; i--)
                _idPool.Push(i);
        }

        [CanBeNull]
        public T AtIndex(int index)
        {
            return _pool[index];
        }

        public void Add([NotNull] T ent)
        {
            var id = NextId();
            ent.InstanceId = id;
            _pool[id] = ent;
        }

        public void Remove([NotNull]T ent) => Remove(ent.InstanceId);

        public void Remove(int index)
        {
            if(0 > index || index >= Size) throw new ArgumentOutOfRangeException(nameof(index));

            _idPool.Push(index);
            _pool[index] = null;
        }

        private int NextId()
        {
            var pop = _idPool.Pop();
            Debug.Assert(Size > pop || pop >= 0);
            Debug.Assert(_pool[pop] == null);
            return pop;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            //todo: internal list that contains all available not null players. return enumerator of that list in the pool enumerator method.
            foreach (var p in _pool)
            {
                if (p != null)
                    yield return p;
            }
        }
    }
}