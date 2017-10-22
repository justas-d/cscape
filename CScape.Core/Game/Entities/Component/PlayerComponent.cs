using System;
using CScape.Core.Extensions;
using CScape.Core.Game.Entities.Message;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Component
{
    public sealed class PlayerComponent : EntityComponent, IPlayerComponent
    {
        public enum Title : byte
        {
            Normal = 0,
            Moderator = 1,
            Admin = 2
        }

        // TODO : fill player component with data from DB

        public int TitleId { get; } = (int)Title.Normal;

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

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case SysMessage.DestroyEntity:
                {
                    _destroyCallback?.Invoke(this);
                    break;
                }

                case (int)MessageId.JustDied:
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
            Parent.GetNetwork()?.DropConnection();
        }

        public bool Equals(IPlayerComponent other)
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
