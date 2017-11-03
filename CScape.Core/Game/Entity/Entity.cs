using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Game.Entity.Message;
using CScape.Models;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    public sealed class Entity : IEntity
    {
        public string Name { get; }

        public IEntityHandle Handle { get; }

        public IGameServer Server => Handle.System.Server;

        public ILogger Log { get; }

        public IEntityComponentContainer Components { get; }

        public Entity([NotNull] string name, [NotNull] EntityHandle handle)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Handle = handle ?? throw new ArgumentNullException(nameof(handle));

            Components = new EntityComponentContainer();
            Log = Server.Services.ThrowOrGet<ILogger>();
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


