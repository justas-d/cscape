using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.NewEntity
{
    public sealed class EntityFactory
    {
        public enum TransformType
        {
            None,
            Server,
            Client,
        }
        
        public const int IdBits = sizeof(int) - GenerationBits;
        public const int GenerationBits = 8;
        public int IdThreshold { get; }

        private int _idTop = 0;
        private readonly List<int> _idQueue;
        private readonly Dictionary<int, int> _generationTracker = new Dictionary<int, int>(); // id  -> generation

        private readonly Dictionary<EntityHandle, Entity> _entities = new Dictionary<EntityHandle, Entity>();

        [NotNull]
        public IGameServer Server { get; }

        public EntityFactory([NotNull] IGameServer server, int idThreshold = 1024)
        {
            Debug.Assert(idThreshold > 0);

            Server = server ?? throw new ArgumentNullException(nameof(server));

            IdThreshold = idThreshold;
            _idQueue = new List<int>(IdThreshold);
        }

        public bool IsDead(EntityHandle handle)
        {
            if (_generationTracker.ContainsKey(handle.Id))
            {
                return _generationTracker[handle.Id] != handle.Generation;
            }
            else
                return true;
        }

        public EntityHandle Create([NotNull] string name, bool useClientTransform = false)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            int id;
            if (_idQueue.Count >= IdThreshold)
            {
                Debug.Assert(_idQueue.Count > 0);
                id = Enumerable.First<int>(_idQueue);
            }
            else
            {
                id = _idTop++;
            }

            if(!_generationTracker.ContainsKey(id))
                _generationTracker.Add(id, 0);

            var handle = new EntityHandle(this, _generationTracker[id], id);
            var entity = new Entity(name, handle);

            ITransform transform = useClientTransform
                ? new ClientTransform(entity)
                : new ServerTransform(entity);

            entity.AddComponent(transform);
            
            Debug.Assert(!_entities.ContainsKey(handle));
            _entities.Add(handle, entity);

            return handle;
        }

        public void Destroy([NotNull] EntityHandle handle)
        {
            if (handle == null) throw new ArgumentNullException(nameof(handle));
            if (IsDead(handle)) return;

            Debug.Assert(_generationTracker.ContainsKey(handle.Id));
            Debug.Assert(_entities.ContainsKey(handle));

            // advance the generation for this id
            _generationTracker[handle.Id] += 1;

            _entities.Remove(handle);
        }

        public Entity Get([NotNull] EntityHandle entityHandle)
        {
            if (entityHandle == null) throw new ArgumentNullException(nameof(entityHandle));
            if(IsDead(entityHandle)) throw new DestroyedEntityDereference(entityHandle);

            return _entities[entityHandle];
        }
    }
}