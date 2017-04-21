using System;
using CScape;
using CScape.Game.Entity;
using JetBrains.Annotations;

namespace cscape_dev
{
    public class SaveData : IPlayerSaveData
    {
        public int Id { get; set; }
        public string PasswordHash { get; set; }
        public string Username { get; set; }
        public byte TitleIcon { get; set; }

        public ushort X { get; set; }
        public ushort Y { get; set; }
        public byte Z { get; set; }

        /// <summary>
        /// SQLite constructor
        /// </summary>
        public SaveData()
        {

        }

        /// <summary>
        /// New player constructor
        /// </summary>
        public SaveData([NotNull] string username, [NotNull] string pwdHash)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            PasswordHash = pwdHash ?? throw new ArgumentNullException(nameof(pwdHash));
        }

        /// <summary>
        /// Save existing player constructor
        /// </summary>
        public SaveData(Player player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (string.IsNullOrEmpty(player.Username)) throw new ArgumentNullException(nameof(player.Username));
            if (string.IsNullOrEmpty(player.PasswordHash)) throw new ArgumentNullException(nameof(player.PasswordHash));

            Id = player.Id;
            PasswordHash = player.PasswordHash;
            Username = player.Username;
            TitleIcon = player.TitleIcon;
            X = player.Position.X;
            Y = player.Position.Y;
            Z = player.Position.Z;
        }
    }
}