using System;
using System.Collections.Immutable;
using CScape.Core.Extensions;
using CScape.Models;
using CScape.Models.Data;
using JetBrains.Annotations;

namespace CScape.Core.Json
{
    public class InMemoryConfigurationService : IConfigurationService
    {
        private ImmutableDictionary<string, string> _lookup = ImmutableDictionary<string, string>.Empty;

        protected ImmutableDictionary<string, string> Lookup
        {
            get => _lookup;
            set => _lookup = value;
        }

        private readonly Lazy<ILogger> _log;
        public ILogger Log => _log.Value;


        public InMemoryConfigurationService(
            [NotNull] IServiceProvider services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            _log = services.GetLazy<ILogger>();
        }


        public bool Add(string key, string value)
        {
            return ImmutableInterlocked.TryAdd(ref _lookup, key, value);
        }

        public string Get(string key)
        {
            _lookup.TryGetValue(key, out var retval);
            return retval;
        }

        public virtual void Reload()
        {
        }

        public virtual void Dispose()
        {
        }
    }
}