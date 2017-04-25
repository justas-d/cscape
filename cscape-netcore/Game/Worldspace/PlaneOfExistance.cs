using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Game.Worldspace
{
    public class PlaneOfExistance : IEnumerable<AbstractEntity>
    {
        [NotNull]
        public GameServer Server { get; }
        public bool IsOverworld => Server.Overworld == this;

        private readonly EntityPool<AbstractEntity> _entityPool;
        private bool _isFreed;

        public PlaneOfExistance([NotNull] GameServer server)
        {
            Server = server ?? throw new ArgumentNullException(nameof(server));

            _entityPool = new EntityPool<AbstractEntity>();
            Server.Entities.Add(_entityPool);
        }

        ~PlaneOfExistance()
        {
            if (!_isFreed && !IsOverworld)
            {
                Server.Log.Warning(this, "Finalizer called on unfreed PoE.");
                Free();
            }
        }

        public void Free()
        {
            if (_isFreed) return;
            if(IsOverworld) return;

            Server.Entities.Remove(_entityPool);
            _isFreed = true;
        }

        /// <summary>
        /// !!Should only be called by AbstractEntity.
        /// </summary>
        internal void RemoveEntity([NotNull] AbstractEntity ent)
        {
            if (ent == null) throw new ArgumentNullException(nameof(ent));

            if(!ContainsEntity(ent))
                return;

            _entityPool.Remove(ent);
        }

        /// <summary>
        /// !!Should only be called by AbstractEntity.
        /// </summary>
        internal void AddEntity([NotNull] AbstractEntity ent)
        {
            // an entity can only belong to one poe at a time.
            if (ent.PoE != this)
                Debug.Fail("PoE tried to AddEntity on a entity that is in a different PoE.");

            if (ent == null) throw new ArgumentNullException(nameof(ent));
            if (ContainsEntity(ent))
                return;

            _entityPool.Add(ent);

            // if we're adding a new observer, push them our observables
            var observer = ent as IObserver;
            if (observer != null)
            {
                observer.Observatory.Clear();
                foreach(var e in this)
                    observer.Observatory.PushObservable(e);
            }
        }

        public bool ContainsEntity([NotNull] AbstractEntity obs)
        {
            if (obs == null) throw new ArgumentNullException(nameof(obs));
            return _entityPool.ContainsId(obs.UniqueEntityId);
        }

        public IEnumerator<AbstractEntity> GetEnumerator()
            => _entityPool.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}