using System;
using System.Collections.Generic;
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
        private readonly Dictionary<IEntityHandle, IEntity> _entities = new Dictionary<IEntityHandle, IEntity>();
        private readonly Dictionary<IEntityHandle, IEntity> _createQueue = new Dictionary<IEntityHandle, IEntity>();
        private readonly HashSet<IEntityHandle> _deleteQueue = new HashSet<IEntityHandle>();

        public IReadOnlyDictionary<IEntityHandle, IEntity> All => _entities;

        public EntitySystem([NotNull] IGameServer server, int idThreshold = 1024)
        {
            Debug.Assert(idThreshold > 0);

            Server = server ?? throw new ArgumentNullException(nameof(server));

            IdThreshold = idThreshold;
            _idQueue = new List<int>(IdThreshold);
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

            var t = new Transform(entity);
            entity.Components.Add(t);
            entity.Components.Add<ITransform>(t);

            // we must delegate the adding of the entity to the entity list to
            // the end of the frame to allow for the creation of entities during
            // the iteration of all living entities.
            // even if an entity is in the create queue, we will still regard it as alive.
            _createQueue.Add(entity.Handle, entity);
            
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

        private void DeleteEntity([NotNull] IEntityHandle handle)
        {
            if (handle == null) throw new ArgumentNullException(nameof(handle));
            Debug.Assert(_entities.ContainsKey(handle));

            // advance the generation
            _generationTracker[handle.Id] += 1;

            // remove ent
            _entities.Remove(handle);
        }

        private void FlushDeleteQueue()
        {
            foreach (var handle in _deleteQueue)
                DeleteEntity(handle);
            
            _deleteQueue.Clear();
        }

        private void AddEntity([NotNull] IEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            Debug.Assert(!_entities.ContainsKey(entity.Handle));

            _entities.Add(entity.Handle, entity);
        }

        private void FlushCreateQueue()
        {
            // flush entities in the create queue to the main entity map
            foreach (var ent in _createQueue.Values)
                AddEntity(ent);
            
            _createQueue.Clear();
        }

        public void PostFrame()
        {
            FlushDeleteQueue();
            FlushCreateQueue();
        }

        public IEntity Get(IEntityHandle entityHandle)
        {
            if (entityHandle == null) throw new ArgumentNullException(nameof(entityHandle));
            if(IsDead(entityHandle)) throw new DestroyedEntityDereference(entityHandle);

            // we've got to check the _entities list and the create queue for this one
            if (_entities.ContainsKey(entityHandle))
                return _entities[entityHandle];
            else
                return _createQueue[entityHandle];
        }

        public bool IsQueuedForDeath(IEntityHandle handle) => _deleteQueue.Contains(handle);

        public bool IsDead(IEntityHandle handle)
        {
            if (_generationTracker.ContainsKey(handle.Id))
            {
                return _generationTracker[handle.Id] != handle.Generation;
            }
            else
                return true;
        }

    }
}