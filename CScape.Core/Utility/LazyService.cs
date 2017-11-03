using System;
using System.Diagnostics;
using CScape.Core.Extensions;
using JetBrains.Annotations;

namespace CScape.Core.Utility
{
    // todo : use LazyService everywhere
    /// <summary>
    /// Provides lazy-initialized way of retrieving <typeparam name="T"></typeparam> mapped services from an <see cref="IServiceProvider"/>.
    /// </summary>
    public struct LazyService<T> where T : class
    {
        public bool IsValueCreated { get; private set; }

        [CanBeNull]
        private T _value;

        [NotNull]
        public T Value
        {
            get
            {
                CreateValueIfNeeded();
                Debug.Assert(_value != null);
                return _value;
            }
        }

        [NotNull]
        private readonly IServiceProvider _services;

        public LazyService([NotNull] IServiceProvider services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            IsValueCreated = false;
            _value = null;
        }

        private void CreateValueIfNeeded()
        {
            if (IsValueCreated)
                return;

            IsValueCreated = true;
            _value = _services.ThrowOrGet<T>();
        }
    }
}
