using System;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Component
{
    public sealed class PlayerComponent : IEntityComponent, IEquatable<PlayerComponent>
    {
        [NotNull] private readonly Action<PlayerComponent> _destroyCallback;

        public int PlayerId { get; }

        [NotNull]
        public Entity Parent { get; }

        public int Priority { get; } = 1;

        [NotNull]
        public string Username { get; }

        public PlayerComponent(
            [NotNull] Entity parent,
            [NotNull] string username,
            int playerId,
            [NotNull] Action<PlayerComponent> destroyCallback)
        {
            _destroyCallback = destroyCallback ?? throw new ArgumentNullException(nameof(destroyCallback));
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
                    _destroyCallback(this);
                    break;
                }

                case EntityMessage.EventType.JustDied:
                {
                    // TODO : handle death in PlayerComponent
                    break;
                }

            }
        }

        /// <summary>
        /// Logs out (destroys) the entity only if it's safe for the player to log out.
        /// </summary>
        /// <returns>True - the player logged out, false otherwise</returns>
        public bool TryLogout()
        {
            // TODO : check if the player can log out. (in combat or something)

            LogoffPacket.Static.Send(Connection.OutStream);

            Parent.Handle.System.Destroy(Parent.Handle);
            return true;
        }

        /// <summary>
        /// Forcefully drops the connection. 
        /// Keeps the player alive in the world.
        /// Should only be used when something goes wrong.
        /// </summary>
        public void ForcedLogout()
        {
            var net = Parent.Components.Get<NetworkingComponent>();

            LogoffPacket.Static.Send(Connection.OutStream);
            net?.DropConnection();
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

        public override string ToString()
        {
            return $"Player \"{Username}\" PID: {PlayerId})";
        }

        public override int GetHashCode()
        {
            return Username.GetHashCode() * 31;
        }
    }
}
