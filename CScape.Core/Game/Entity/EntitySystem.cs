using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Game.Entity.Message;
using CScape.Models;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Entity.Exceptions;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    public sealed class EntitySystem : IEntitySystem
    { 
        public const int GenerationBits = 8;

        public int IdThreshold { get; }
        public IGameServer Server { get; }

        private int _idTop = 0;
        private readonly List<int> _idQueue;
        private readonly Dictionary<int, int> _generationTracker = new Dictionary<int, int>(); // id  -> generation
        private ImmutableDictionary<IEntityHandle, IEntity> _entities = ImmutableDictionary<IEntityHandle, IEntity>.Empty;
        private readonly HashSet<IEntityHandle> _deleteQueue = new HashSet<IEntityHandle>();

        public IReadOnlyDictionary<IEntityHandle, IEntity> All => _entities;

        // todo : make Core.EntitySystem take an IServiceProvider
        public EntitySystem([NotNull] IGameServer server, int idThreshold = 1024)
        {
            Debug.Assert(idThreshold > 0);

            Server = server ?? throw new ArgumentNullException(nameof(server));

            IdThreshold = idThreshold;
            _idQueue = new List<int>(IdThreshold);
        }

        private void AddEntityInternally([NotNull] IEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            Debug.Assert(!_entities.ContainsKey(entity.Handle));

            _entities = _entities.Add(entity.Handle, entity);
        }

        private void RemoveEntityIntenally([NotNull] IEntityHandle handle)
        {
            if (handle == null) throw new ArgumentNullException(nameof(handle));
            Debug.Assert(_entities.ContainsKey(handle));

            // advance the generation
            _generationTracker[handle.Id] += 1;

            // remove ent
            _entities = _entities.Remove(handle);
        }

        public IEntityHandle Create(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            int id;
            if (_idQueue.Count >= IdThreshold)
            {
                Debug.Assert(_idQueue.Count > 0);
                id = _idQueue.First();
            }
            else
            {
                id = _idTop++;
            }

            if (!_generationTracker.ContainsKey(id))
                _generationTracker.Add(id, 0);

            var handle = new EntityHandle(this, _generationTracker[id], id);
            var entity = new Entity(name, handle);

            var t = new TransformComponent(entity);
            entity.Components.Add(t);
            entity.Components.Add<ITransform>(t);

            AddEntityInternally(entity);
            
            return handle;
        }
   
        public bool Destroy([NotNull] IEntityHandle handle)
        {
            if (handle == null) throw new ArgumentNullException(nameof(handle));
            if (IsDead(handle)) return false;

            Debug.Assert(_generationTracker.ContainsKey(handle.Id));

            // don't destroy entities that dont exist
            if (!_entities.TryGetValue(handle, out var ent))
                return false;
            
            // dont destroy entities that are already in the destroy queue
            // .Add will return false if we failed to add an element
            if (!_deleteQueue.Add(handle))
                return false;

            // ent is queued for death, send the notif
            ent.SendMessage(NotificationMessage.QueuedForDeath);
            
            return true;
        }

        public void DestroyAll()
        {
            foreach (var handle in _entities.Keys)
                Destroy(handle);
        }

        private void FlushDeleteQueue()
        {
            foreach (var handle in _deleteQueue)
                RemoveEntityIntenally(handle);
            
            _deleteQueue.Clear();
        }

        public void PostFrame()
        {
            FlushDeleteQueue();
        }

        public IEntity Get(IEntityHandle entityHandle)
        {
            if (entityHandle == null) throw new ArgumentNullException(nameof(entityHandle));
            if(IsDead(entityHandle)) throw new DestroyedEntityDereference(entityHandle);

            return _entities[entityHandle];
        }

        public bool IsDead(IEntityHandle handle)
        {
            if (_generationTracker.ContainsKey(handle.Id))
                return _generationTracker[handle.Id] != handle.Generation;

            return true;
        }
    }
}