using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CScape.Game.Entity
{
    public class AggregateEntityPool<T> : IEnumerable<T> where T : class, IEntity
    {
        private readonly HashSet<EntityPool<T>> _pools = new HashSet<EntityPool<T>>();

        public void Add(EntityPool<T> newPool)
        {
            _pools.Add(newPool);
        }

        public void Remove(EntityPool<T> delPool)
        {
            _pools.Remove(delPool);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _pools.SelectMany(pool => pool).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}