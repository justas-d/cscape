using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CScape.Core;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;

namespace CScape.Basic.Server
{
    public abstract class AbstractEntityRegistry<TKey, T> : IEntityRegistry<TKey, T> where T : class, IEntity
    {
        private ImmutableDictionary<TKey, T> _all = ImmutableDictionary<TKey, T>.Empty;
        public IReadOnlyDictionary<TKey, T> All => _all;

        private readonly ILogger _log;

        public AbstractEntityRegistry(IServiceProvider services)
        {
            _log = services.ThrowOrGet<ILogger>();
        }

        public T GetById(TKey id)
        {
            if (!_all.ContainsKey(id))
            {
                _log.Warning(this, $"Attempted to get unregistered id: {id}");
                return null;
            }

            return _all[id];
        }

        public void Register(T ent)
        {
            _log.Debug(this, $"Registering entity: {ent}.");
            var id = GetId(ent);

            if (All.ContainsKey(id))
            {
                _log.Warning(this, $"Tried to register existing id: {id} ent: {ent}.");
                return;
            }

            _all = _all.Add(id, ent);
        }

        public void Unregister(T ent)
        {
            _log.Debug(this, $"Unregistering entity: {ent}.");
            var id = GetId(ent);

            if (!_all.ContainsKey(id))
            {
                _log.Warning(this, $"Tried to unregister player that is not registered: id: {id} ent: {ent}.");
                return;
            }

            _all = _all.Remove(id);
        }

        protected abstract TKey GetId(T val);
    }
}