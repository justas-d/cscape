using System;
using CScape;
using CScape.Game.Entity;
using JetBrains.Annotations;

namespace cscape_dev
{
    public class SaveData : IPlayerSaveData
    {
        public int DatabaseId { get; set; }
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
        public SaveData([NotNull] Player player)
        {
            Update(player);
        }

        public void Update([NotNull] IPlayerSaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(data.Username)) throw new ArgumentNullException(nameof(data.Username));
            if (string.IsNullOrEmpty(data.PasswordHash)) throw new ArgumentNullException(nameof(data.PasswordHash));

            DatabaseId = data.DatabaseId;
            PasswordHash = data.PasswordHash;
            Username = data.Username;
            TitleIcon = data.TitleIcon;
            X = data.X;
            Y = data.Y;
            Z = data.Z;
        }
    }
}