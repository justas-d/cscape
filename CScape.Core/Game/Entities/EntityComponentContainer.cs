using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities
{
    public sealed class EntityComponentContainer<TComponent>
        : IEnumerable<TComponent>, IEntityComponentContainer<TComponent> where TComponent : class, IEntityComponent
    {
        [NotNull]
        public Entity Parent { get; }

        private readonly Dictionary<Type, TComponent> _lookup
            = new Dictionary<Type, TComponent>();

        // TODO : write tests for entity fragment sorting

        // we set this to Enimerable.Empty because as soon as this container is modified,
        // we immediatelly call Sort(), which assigns a sorted, by IEntityComponent.Priority, IEnumerable
        [NotNull]
        public IEnumerable<TComponent> All { get; private set; } = Enumerable.Empty<TComponent>();



        public EntityComponentContainer([NotNull] Entity parent)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        private void Sort()
        {
            All = _lookup.Values.OrderBy(f => f.Priority);
        }

        public void Add<T>([NotNull] T fragment)
            where T : class, TComponent
        {
            if (fragment == null) throw new ArgumentNullException(nameof(fragment));

            var type = typeof(T);

            if (Contains<T>())
                throw new EntityComponentAlreadyExists(type);

            _lookup.Add(type, fragment);
            Sort();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains<T>()
            where T : class, TComponent
            => _lookup.ContainsKey(typeof(T));

        [CanBeNull]
        public T Get<T>()
            where T : class, TComponent
        {
            var type = typeof(T);

            if (!Contains<T>())
                return null;

            return (T)_lookup[type];
        }

        [NotNull]
        public T AssertGet<T>()
            where T : class, TComponent
        {
            var val = Get<T>();
#if DEBUG
            Debug.Assert(val != null);
#else
                
                if (val == null)
                    throw new InvalidOperationException(
                        $"Attempted to get component that does not exist. Comp: {typeof(T).Name} Ent: {Parent}");
#endif

            return val;
        }

        public void Remove<T>()
            where T : class, TComponent
        {
            // TODO : assert that fragment requirements are still satisfied after removal of fragment

            var type = typeof(T);

            if (!Contains<T>())
                return;

            var statusLookup = _lookup.Remove(type);

            Debug.Assert(statusLookup);
        }

        public IEnumerator<TComponent> GetEnumerator() => All.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}