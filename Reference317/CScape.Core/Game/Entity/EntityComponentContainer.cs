using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    public sealed class EntityComponentContainer : IEntityComponentContainer
    {
        private ImmutableDictionary<Type, IEntityComponent> _lookup 
            = ImmutableDictionary<Type, IEntityComponent>.Empty;

        // TODO : write tests for entity component sorting

        [CanBeNull]
        private List<IEntityComponent> _sorted;

        public IReadOnlyDictionary<Type, IEntityComponent> Lookup => _lookup;

        public IEnumerable<IEntityComponent> GetSorted()
        {
            if (_sorted == null)
            {
                // flatten to a list in order to avoid reordering every GetSorted call
                _sorted = _lookup.Values.OrderBy(f => f.Priority).ToList();
            }

            return _sorted;
        }

        private void Sort()
        {
            _sorted = null;
        }

        public bool Add<T>(T fragment)
            where T : class, IEntityComponent
        {
            if (fragment == null) throw new ArgumentNullException(nameof(fragment));

            var type = typeof(T);

            if (Contains<T>())
                return false;

            _lookup = _lookup.Add(type, fragment);
            Sort();
            return true;
        }

        public bool Add(Type type, IEntityComponent component)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (component == null) throw new ArgumentNullException(nameof(component));

            if (type == typeof(IEntityComponent))
                return false;

            if (Contains(type))
                return false;

            _lookup = _lookup.Add(type, component);
            Sort();
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains<T>()
            where T : class, IEntityComponent
            => _lookup.ContainsKey(typeof(T));

        public bool Contains(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return _lookup.ContainsKey(type);
        }

        public T Get<T>()
            where T : class, IEntityComponent
        {
            var type = typeof(T);

            if (!Contains<T>())
                return null;

            return (T)_lookup[type];
        }

        public IEntityComponent Get(Type type)
        {
            if (!Contains(type))
                return null;
            return _lookup[type];
        }

        public T AssertGet<T>()
            where T : class, IEntityComponent
        {
            var val = Get<T>();
#if DEBUG
            Debug.Assert(val != null);
#else
                
                if (val == null)
                    throw new InvalidOperationException(
                        $"Attempted to get component that does not exist. Comp: {typeof(T).Name}");
#endif

            return val;
        }

        public bool Remove<T>()
            where T : class, IEntityComponent
        {
            // TODO : assert that fragment requirements are still satisfied after removal of fragment

            var type = typeof(T);

            if (!Contains<T>())
                return false;

            _lookup = _lookup.Remove(type);
            return true;
        }

        public bool Remove(Type type)
        {
            if (!Contains(type))
                return false;

            _lookup = _lookup.Remove(type);
            return true;
        }
    }
}