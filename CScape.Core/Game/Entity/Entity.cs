using System;
using System.Collections.Generic;
using System.Reflection;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Utility;
using CScape.Models;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    public sealed class Entity : IEntity
    {
        private Lazy<ILogger> _log;
        private Lazy<IGameServer> _server;

        public IGameServer Server => _server.Value;
        public ILogger Log => _log.Value;

        public string Name { get; }
        public IEntityHandle Handle { get; }
        public IEntityComponentContainer Components { get; }

        public Entity(
            [NotNull] string name, 
            [NotNull] IEntityHandle handle,
            [NotNull] IServiceProvider services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Handle = handle ?? throw new ArgumentNullException(nameof(handle));

            Components = new EntityComponentContainer();
            _log = services.GetLazy<ILogger>();
            _server = services.GetLazy<IGameServer>();
        }

        public void SendMessage(IGameMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var handledComponentTypes = new HashSet<Type>();
           
            foreach (var component in Components.GetSorted())
            {
                var componentType = component.GetType();

                if (handledComponentTypes.Contains(componentType))
                    continue;
                    
                component.ReceiveMessage(message);

                handledComponentTypes.Add(componentType);
            }
        }

        public void SystemMessage(string msg, ulong flags = (ulong)SystemMessageFlags.Normal)
        {
            if (string.IsNullOrEmpty(msg)) return;
            SendMessage(new SystemMessage(msg, flags));
        }

        public bool AreComponentRequirementsSatisfied(out string message)
        {
            message = null;
            foreach (var component in Components.Lookup.Values)
            {
                foreach (var attrib in
                    component.GetType().GetTypeInfo().GetCustomAttributes<RequiresComponent>())
                {
                    // look for required attrib
                    var match = Components.Get(attrib.ComponentType);
                    if (match == null)
                    {
                        message = $"{component.GetType().Name} requires component of type {attrib.ComponentType.Name} to be attached to the entity but it is not.";
                        return false;
                    }
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            return Handle.GetHashCode() * 13;
        }

        public override string ToString()
        {
            return $"Entity \"{Name}\" {Handle}";
        }

        public bool Equals(IEntity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Handle.Equals(other.Handle);
        }

        public bool Equals(IEntityHandle other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Handle.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Entity && Equals((Entity)obj);
        }
    }
}


