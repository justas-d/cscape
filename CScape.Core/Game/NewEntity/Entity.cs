using System;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;

namespace CScape.Core.Game.NewEntity
{
    public class EntityComponentError : Exception
    {
        public Type Type { get; }

        public EntityComponentError(Type type)
        {
            Type = type;
        }

        public override string ToString()
        {
            return $"Type: {Type} {base.ToString()}";
        }
    }

    public class EntityComponentNotFound : EntityComponentError
    {
        public EntityComponentNotFound(Type type) : base(type)
        {
        }
    }

    public class EntityComponentAlreadyExists : EntityComponentError
    {
        public EntityComponentAlreadyExists(Type type) : base(type)
        {
        }
    }

    public sealed class Entity
    {
        [NotNull]
        public string Name { get; }

        [NotNull]
        public EntityHandle Handle { get; }

        [NotNull]
        public IGameServer Server => Handle.Factory.Server;

        public ITransform GetTransform() => GetComponent<ITransform>();

        private readonly Dictionary<Type, IEntityComponent> _components = new Dictionary<Type, IEntityComponent>();

        public Entity([NotNull] string name, [NotNull] EntityHandle handle)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Handle = handle ?? throw new ArgumentNullException(nameof(handle));
        }

        public void AddComponent<T>(T component) where T : IEntityComponent
        {
            var type = typeof(T);
            if (_components.ContainsKey(type))
                throw new EntityComponentAlreadyExists(type);
            
            _components.Add(type, component);
        }

        public T GetComponent<T>() where T : IEntityComponent
        {
            var type = typeof(T);
            if (!_components.ContainsKey(type))
                throw new EntityComponentNotFound(type);
            
            return (T)_components[type];
        }
    }

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
                id = _idQueue.First();
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

    public interface IEntityComponent
    {
        Entity Parent { get; }

        void Update([NotNull]IMainLoop loop);
    }

    public class DestroyedEntityDereference : Exception
    {
        public EntityHandle Handle { get; }

        public DestroyedEntityDereference(EntityHandle handle)
        {
            Handle = handle;
        }

        public override string ToString()
        {
            return $"Handle: {Handle} Exception: {base.ToString()}";
        }
    }

    public sealed class EntityHandle : IEquatable<EntityHandle>
    {
        public EntityFactory Factory { get; }
        public int Generation { get; }
        public int Id { get; }

        private readonly int _baked; 

        public EntityHandle([NotNull] EntityFactory factory, int generation, int id)
        {
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            Generation = generation;
            Id = id;

            _baked = (Id << EntityFactory.GenerationBits) | Generation;
        }

        bool IsDead() => Factory.IsDead(this);
        Entity Get() => Factory.Get(this);
        
        public bool Equals(EntityHandle other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return _baked == other._baked;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;

            return Equals((EntityHandle) obj);
        }

        public override int GetHashCode() => _baked;
        public override string ToString() => $"Entity handle: Id: {Id} Generation: {Generation} Baked: {_baked}";
    }
}

