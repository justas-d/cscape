using System;
using JetBrains.Annotations;
using System.Collections.Generic;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;

namespace CScape.Core.Game.NewEntity
{
    public sealed class Entity : IEquatable<Entity>
    {
        [NotNull]
        public string Name { get; }

        [NotNull]
        public EntityHandle Handle { get; }

        [NotNull]
        public IGameServer Server => Handle.System.Server;

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

        public bool Equals(Entity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Handle.Equals(other.Handle);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Entity && Equals((Entity) obj);
        }

        /// <summary>
        /// Sends out an <see cref="EntityMessage"/> to each and every component
        /// of this entity. The sender of the message will not receive the message.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        public void SendMessage([NotNull] EntityMessage message)
        {
            foreach (var comp in _components.Values)
            {
                if(comp != message.Sender)
                    comp.ReceiveMessage(message);
            }
        }

        public override int GetHashCode()
        {
            return Handle.GetHashCode() * 13;
        }

        public override string ToString()
        {
            return $"Entity \"{Name}\" {Handle}";
        }
    }
}

