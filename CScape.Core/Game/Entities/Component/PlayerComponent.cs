using System;
using CScape.Core.Network.Entity.Component;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Component
{
    public sealed class PlayerComponent :
        EntityComponent, IEquatable<PlayerComponent>, IEquatable<string>
    {
        public enum Title : byte
        {
            Normal = 0,
            Moderator = 1,
            Admin = 2
        }

        // TODO : fill player component with data from DB

        public Title TitleIcon { get; }

        [CanBeNull] private readonly Action<PlayerComponent> _destroyCallback;

        public int PlayerId { get; }

        public override int Priority { get; } = 1;

        [NotNull]
        public string Username { get; }

        public PlayerComponent(
            [NotNull] Entity parent,
            [NotNull] string username,
            int playerId,
            [CanBeNull] Action<PlayerComponent> destroyCallback)
            :base(parent)
        {
            _destroyCallback = destroyCallback ?? throw new ArgumentNullException(nameof(destroyCallback));
            PlayerId = playerId;
            Username = username ?? throw new ArgumentNullException(nameof(username));
        }

        public override void ReceiveMessage(GameMessage msg)
        {
            switch (msg.Event)
            {
                case GameMessage.Type.DestroyEntity:
                {
                    _destroyCallback?.Invoke(this);
                    break;
                }

                case GameMessage.Type.JustDied:
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
            net?.DropConnection();
        }

        public bool Equals(PlayerComponent other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Username);
        }

        public bool Equals(string other)
        {
            return string.Equals(Username, other, StringComparison.OrdinalIgnoreCase);
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
