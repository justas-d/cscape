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

        private int _idTop = 0;
        private readonly List<int> _idQueue;
        private readonly Dictionary<int, int> _generationTracker = new Dictionary<int, int>(); // id  -> generation

        private readonly Dictionary<IEntityHandle, IEntity> _entities 
            = new Dictionary<IEntityHandle, IEntity>();

        public IReadOnlyDictionary<IEntityHandle, IEntity> All => _entities;

        [NotNull]
        public IGameServer Server { get; }

        public EntitySystem([NotNull] IGameServer server, int idThreshold = 1024)
        {
            Debug.Assert(idThreshold > 0);

            Server = server ?? throw new ArgumentNullException(nameof(server));

            IdThreshold = idThreshold;
            _idQueue = new List<int>(IdThreshold);
        }

        public bool IsDead(IEntityHandle handle)
        {
            if (_generationTracker.ContainsKey(handle.Id))
            {
                return _generationTracker[handle.Id] != handle.Generation;
            }
            else
                return true;
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

            Debug.Assert(!_entities.ContainsKey(handle));
            _entities.Add(handle, entity);

            return handle;
        }
   
        public bool Destroy([NotNull] IEntityHandle handle)
        {
            if (handle == null) throw new ArgumentNullException(nameof(handle));
            if (IsDead(handle)) return false;

            Debug.Assert(_generationTracker.ContainsKey(handle.Id));

            if (!_entities.TryGetValue(handle, out var ent))
                return false;

            ent.SendMessage(NotificationMessage.DestroyEntity);

            // advance the generation for this id
            _generationTracker[handle.Id] += 1;

            _entities.Remove(handle);

            return true;
        }

        public IEntity Get(IEntityHandle entityHandle)
        {
            if (entityHandle == null) throw new ArgumentNullException(nameof(entityHandle));
            if(IsDead(entityHandle)) throw new DestroyedEntityDereference(entityHandle);

            return _entities[entityHandle];
        }
    }
}