using System;
using JetBrains.Annotations;

namespace cscape
{
    public sealed class Player : Entity
    {
        public const int MaxUsernameChars = 12;
        public const int MaxPasswordChars = 128;

        public int Id { get; }
        //@TODO: change username feature
        public string Username { get; private set; }
        //@TODO: change password feature
        public string PasswordHash { get; private set; }

        public Player([NotNull] IPlayerSaveData save)
        {
            if (save == null) throw new ArgumentNullException(nameof(save));
            if(string.IsNullOrEmpty(save.Username)) throw new ArgumentNullException(nameof(save.Username));
            if (string.IsNullOrEmpty(save.PasswordHash)) throw new ArgumentNullException(nameof(save.PasswordHash));

            Id = save.Id;
            Username = save.Username;
            PasswordHash = save.PasswordHash;
        }
    }
}