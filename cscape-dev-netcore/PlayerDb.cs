using System;
using System.Threading.Tasks;
using CScape;
using CScape.Game.Entity;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace cscape_dev
{
    public class PlayerDb : DbContext, IPlayerDatabase
    {
        public DbSet<SaveData> SaveData { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename=data.db");
        }

        protected override void OnModelCreating(ModelBuilder model)
        {
            model.Entity<SaveData>()
                .Property(s => s.Username)
                .IsRequired();

            model.Entity<SaveData>()
                .Property(s => s.PasswordHash)
                .IsRequired();
        }

        public async Task<bool> UserExists([NotNull] string username)
        {
            if (username == null) throw new ArgumentNullException(nameof(username));
            return await SaveData.FirstOrDefaultAsync(s => s.Username == username) != null;
        }

        public async Task<IPlayerSaveData> Load([NotNull] string username, [NotNull] string password)
        {
            if (username == null) throw new ArgumentNullException(nameof(username));
            if (password == null) throw new ArgumentNullException(nameof(password));

            var data = await SaveData.FirstOrDefaultAsync(s => s.Username == username);
            if (data == null || !await IsValidPassword(data.PasswordHash, password))
                return null;

            return data;
        }

        public async Task<IPlayerSaveData> Save([NotNull] Player player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));

            var data = new SaveData(player);
            await SaveByData(data);
            return data;
        }

        private async Task SaveByData(SaveData data)
        {
            var existing = await SaveData.FirstOrDefaultAsync(s => s.Id == data.Id);
            if (existing == null)
                SaveData.Add(data);
            else
            {
                existing.Update(data);
            }
            await SaveChangesAsync();
        }

        public async Task<IPlayerSaveData> LoadOrCreateNew([NotNull] string username, [NotNull] string pwd)
        {
            if (username == null) throw new ArgumentNullException(nameof(username));
            if (pwd == null) throw new ArgumentNullException(nameof(pwd));

            if (await UserExists(username))
                return await Load(username, pwd);

            var data = NewPlayer(username, pwd);
            await SaveByData(data);
            return data;
        }

        private SaveData NewPlayer(string username, string pwd)
        {
            return new SaveData(username, pwd)
            {
                TitleIcon = 0,
                X = 3222,
                Y = 3218,
                Z = 0
                // todo : player defaults here
            };
        }

        public Task<bool> IsValidPassword([NotNull] string pwdHash, [NotNull] string pwd)
        {
            if (pwdHash == null) throw new ArgumentNullException(nameof(pwdHash));
            if (pwd == null) throw new ArgumentNullException(nameof(pwd));

            // NET Core has no bindings for libsodium, so let's just store them in plaintext.
            // TODO: IF YOU ARE DEVELOPING A SERVER FOR PRODUCTION, IMPLEMENT A PASSWORD HASHING SOLUTION
            // todo: check up on https://github.com/jedisct1/libsodium/issues/504 for libsodium bindings

            return Task.FromResult(pwdHash.Equals(pwd, StringComparison.Ordinal));
        }
    }
}