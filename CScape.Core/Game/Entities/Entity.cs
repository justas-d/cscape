using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Entities.Message;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities
{
    public sealed class Entity : IEquatable<Entity>, IEnumerable<IEntityComponent>, IEntity
    {


        [NotNull]
        public string Name { get; }

        [NotNull]
        public EntityHandle Handle { get; }

        [NotNull]
        public IGameServer Server => Handle.System.Server;

        public ILogger Log => Handle.System.Server.Services.ThrowOrGet<ILogger>();

        public ServerTransform GetTransform() => Components.Get<ServerTransform>();

        public EntityComponentContainer<IEntityComponent> Components { get; }

        public Entity([NotNull] string name, [NotNull] EntityHandle handle)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Handle = handle ?? throw new ArgumentNullException(nameof(handle));

            Components = new EntityComponentContainer<IEntityComponent>(this);
        }

        public bool Equals(Entity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Handle.Equals(other.Handle);
        }

        public IEnumerator<IEntityComponent> GetEnumerator()
        {
            return Components.GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Entity && Equals((Entity) obj);
        }

        /// <summary>
        /// Sends out an <see cref="GameMessage"/> to each and every component
        /// of this entity. The sender of the message will not receive the message.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        public void SendMessage([NotNull] GameMessage message)
        {
            foreach (var frag in this)
            {
                frag.ReceiveMessage(message);
            }
        }

        /// <summary>
        /// Sends a system message to the entity.
        /// </summary>
        public void SystemMessage([NotNull] string msg, SystemMessageFlags flags = SystemMessageFlags.None)
        {
            if (string.IsNullOrEmpty(msg)) return;

            SendMessage(
                new GameMessage(
                    null, GameMessage.Type.NewSystemMessage, new SystemMessage(msg, flags)));
        }

        /// <summary>
        /// Asserts that are component dependencies are satisfied.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="EntityComponentNotSatisfied">Thrown, when a component is not satisfied.</exception>
        public void AssertComponentRequirementsSatisfied()
        {
            foreach (var frag in this)
            {
                foreach (var attrib in
                    frag.GetType().GetTypeInfo().GetCustomAttributes<RequiresComponent>())
                {
                    // look for required attrib

                    var match = this.FirstOrDefault(c => c.GetType() == attrib.FragmentType);
                    if (match == null)
                    {
                        throw new EntityComponentNotSatisfied
                            (frag.GetType(), $"Requires fragment of type {attrib.FragmentType.Name} to be attached to the entity but it is not.");
                    }
                }
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}


