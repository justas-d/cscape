using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    public class EntityPool<T> : IEnumerable<T> where T : AbstractEntity
    {
        public int Size => _pool.Count;

        /// <returns>-1 if not bound by a size limit.</returns>>
        public int Capacity { get; }

        // the pool CANNOT contain null keys.
        private readonly Dictionary<uint, T> _pool;

        public EntityPool(int capacity)
        {
            _pool = new Dictionary<uint, T>(capacity);
            Capacity = capacity;
        }

        public EntityPool()
        {
            _pool = new Dictionary<uint, T>();
        }

        [CanBeNull]
        public T GetById(uint id)
        {
            if (!_pool.ContainsKey(id))
                return null;

            return _pool[id];
        }

        public void Add([NotNull] T ent)
        {
            if (ent == null) throw new ArgumentNullException(nameof(ent));
            Debug.Assert(!_pool.ContainsKey(ent.UniqueEntityId));

            _pool[ent.UniqueEntityId] = ent;
        }

        public void Remove([NotNull] T ent)
        {
            if (ent == null) throw new ArgumentNullException(nameof(ent));

            Remove(ent.UniqueEntityId);
        }

        public void Remove(uint id)
        {
            if(!_pool.ContainsKey(id))
                return;

            Debug.Assert(_pool[id] != null);
            _pool.Remove(id);
        }

        public bool ContainsId(uint id)
            => GetById(id) != null;

        [DebuggerStepThrough]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [DebuggerStepThrough]
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var p in _pool.Values)
            {
                Debug.Assert(p != null);
                yield return p;
            }
        }
    }
}