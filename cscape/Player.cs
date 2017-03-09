using System;
using JetBrains.Annotations;

namespace cscape
{
    public interface IPlayerSaveData
    {
        int Id { get; }
        string PasswordHash { get; }
        string Username { get; }
        byte TitleIcon { get; }
    }

    public sealed class Player : Entity
    {
        public const int MaxUsernameChars = 12;
        public const int MaxPasswordChars = 128;

        public int Id { get; }
        //@TODO: change username feature
        public string Username { get; private set; }
        //@TODO: change password feature
        public string PasswordHash { get; private set; }

        public byte TitleIcon { get; set; }

        public Player([NotNull] IPlayerSaveData save)
        {
            if (save == null) throw new ArgumentNullException(nameof(save));
            if(string.IsNullOrEmpty(save.Username)) throw new ArgumentNullException(nameof(save.Username));
            if (string.IsNullOrEmpty(save.PasswordHash)) throw new ArgumentNullException(nameof(save.PasswordHash));

            Id = save.Id;
            Username = save.Username;
            PasswordHash = save.PasswordHash;
            TitleIcon = save.TitleIcon;
        }
    }
}