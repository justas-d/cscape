using System;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Component
{
    public sealed class PlayerComponent : IEntityComponent, IEquatable<PlayerComponent>
    {
        [NotNull] private readonly IPlayerFactory _factory;

        public int PlayerId { get; }

        [NotNull]
        public Entity Parent { get; }

        public int Priority { get; } = 1;

        [NotNull]
        public string Username { get; }

        public PlayerComponent(
            [NotNull] Entity parent,
            [NotNull] IPlayerFactory factory,
            [NotNull] string username,
            int playerId)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            PlayerId = playerId;
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Username = username ?? throw new ArgumentNullException(nameof(username));
        }

        public void Update(IMainLoop loop)
        {

        }

        public void ReceiveMessage(EntityMessage msg)
        {
            switch (msg.Event)
            {
                case EntityMessage.EventType.DestroyEntity:
                {
                    _factory.
                    break;
                }
            }
        }

        public bool Equals(PlayerComponent other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Username, other.Username, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is PlayerComponent && Equals((PlayerComponent) obj);
        }

        public override int GetHashCode()
        {
            return Username.GetHashCode() * 31;
        }
    }
}
