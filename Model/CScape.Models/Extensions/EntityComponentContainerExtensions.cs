using System;
using System.Diagnostics;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Models.Extensions
{
    public static class EntityComponentContainerExtensions
    {
        public static bool Add<T>(this IEntityComponentContainer container, [NotNull] T component)
            where T : class, IEntityComponent
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return container.Add(typeof(T), component);
        }

        public static T Get<T>(this IEntityComponentContainer container)
            where T : class, IEntityComponent
        {
            return (T)container.Get(typeof(T));
        }

        public static T AssertGet<T>(this IEntityComponentContainer container)
            where T : class, IEntityComponent
        {
            var val = container.Get<T>();
            Debug.Assert(val != null);
            return val;
        }

        public static bool Contains<T>(this IEntityComponentContainer container)
            where T : class, IEntityComponent
        {
            return container.Contains(typeof(T));
        }

        public static bool Remove<T>(this IEntityComponentContainer container)
            where T : class, IEntityComponent
        {
            return container.Remove(typeof(T));
        }
    }
}
