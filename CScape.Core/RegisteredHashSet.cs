using System.Collections;
using System.Collections.Generic;
using CScape.Core.Game.Entity;

namespace CScape.Core
{
    // todo : replace RegisteredHashSet with collections of WeakReference?
    public sealed class RegisteredHashSet<T> : IRegisteredCollection, IEnumerable<T> where T : IWorldEntity
    {
        private readonly HashSet<T> _hashset = new HashSet<T>();

        public int Count => _hashset.Count;
        public bool Contains(T obj) => _hashset.Contains(obj);

        public bool Add(IWorldEntity obj)
        {
            if (!_hashset.Add((T) obj))
                return false;   

            // register
            obj.RegisterContainer(this);

            return true;
        }

        public bool Remove(IWorldEntity obj)
        {
            if (!_hashset.Remove((T) obj))
                return false;

            // unregister
            obj.UnregisterContainer(this);

            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _hashset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}