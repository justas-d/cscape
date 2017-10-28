using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Game.Entity.Message;
using CScape.Models;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    public sealed class Entity : IEntity
    {
        public string Name { get; }

        public IEntityHandle Handle { get; }

        public IGameServer Server => Handle.System.Server;

        public ILogger Log => Handle.System.Server.Services.ThrowOrGet<ILogger>();

        public IEntityComponentContainer Components { get; }

        public Entity([NotNull] string name, [NotNull] EntityHandle handle)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Handle = handle ?? throw new ArgumentNullException(nameof(handle));

            Components = new EntityComponentContainer();
        }

        public void SendMessage([NotNull] IGameMessage message)
        {
            var handled = new HashSet<Type>();
            
            foreach (var frag in Components)
            {
                var t = frag.GetType();

                if (handled.Contains(t))
                    continue;
                    

                frag.ReceiveMessage(message);
                handled.Add(frag.GetType());
            }
        }

        public void SystemMessage(string msg, ulong flags = SystemMessageFlags.Normal)
        {
            if (string.IsNullOrEmpty(msg)) return;
            SendMessage(new SystemMessage(msg, flags));
        }

        public bool AreComponentRequirementsSatisfied(out string message)
        {
            message = null;
            foreach (var comp in Components)
            {
                foreach (var attrib in
                    comp.GetType().GetTypeInfo().GetCustomAttributes<RequiresComponent>())
                {
                    // look for required attrib

                    var match = Components.FirstOrDefault(c => c.GetType() == attrib.FragmentType);
                    if (match == null)
                    {
                        message =
                            $"{comp.GetType().Name} requires component of type {attrib.FragmentType.Name} to be attached to the entity but it is not.";
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


