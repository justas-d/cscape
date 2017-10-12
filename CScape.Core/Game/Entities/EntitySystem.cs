using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Entities.Prefab;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities
{
    public sealed class EntitySystem : IEntitySystem
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

        public IReadOnlyDictionary<EntityHandle, Entity> All => _entities;

        [NotNull]
        public IGameServer Server { get; }

        public EntitySystem([NotNull] IGameServer server, int idThreshold = 1024)
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

        public EntityHandle Create([NotNull] string name)
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

            entity.Components.Add(new ServerTransform(entity));

            Debug.Assert(!_entities.ContainsKey(handle));
            _entities.Add(handle, entity);

            return handle;
        }

        public EntityHandle Create([NotNull] EntityPrefab prefab)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));

            var entHandle = Create(prefab.Name);

            // initialize prefabs
            if (prefab.ComponentPrefabs != null)
            {
                foreach (var componentPrefab in prefab.ComponentPrefabs)
                {
                    if (componentPrefab.CachedConstructor == null)
                    {
                        // construct ctor param list
                        var types = componentPrefab.CtorParams
                            .Select(c => c.GetMethodInfo().ReturnType).ToArray();
                        
                        // find ctor
                        var ctor = componentPrefab.InstanceType.GetConstructor(types);
                        if (ctor == null)
                        {
                            var typeStr = new StringBuilder();

                            foreach (var t in types)
                            {
                                typeStr.Append($"{t.Name} ");
                            }

                            // we couldn't find it.
                            throw new EntityPrefabInstantiationFailure(prefab, componentPrefab,
                                $"Couldn't find constructor for type {componentPrefab.InstanceType.Name} with arguments of type: {typeStr}");

                        }

                        componentPrefab.CachedConstructor = ctor;
                    }

                    // construct
                    var component = componentPrefab.CachedConstructor.Invoke(
                        entHandle.Get(), componentPrefab.CtorParams.Select(c => c()).ToArray());
                    
                    // initialize
                    foreach (var setup in componentPrefab.Setups)
                        setup(component);
                    
                }
            }

            // initialize entity
            foreach (var setup in prefab.Setups)
                setup(entHandle.Get());

            Server.Services.ThrowOrGet<ILogger>()
                .Normal(this, $"Instantiated prefab {prefab.Name}");
            
            return entHandle;
        }

        public void Destroy([NotNull] EntityHandle handle)
        {
            if (handle == null) throw new ArgumentNullException(nameof(handle));
            if (IsDead(handle)) return;

            Debug.Assert(_generationTracker.ContainsKey(handle.Id));
            Debug.Assert(_entities.ContainsKey(handle));

            var ent = Get(handle);

            ent.SendMessage(
                new GameMessage(null, GameMessage.Type.DestroyEntity, true));

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