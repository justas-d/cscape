using System;
using System.Linq;
using System.Threading.Tasks;
using CScape;
using CScape.Game.Entity;
using Microsoft.EntityFrameworkCore;

namespace cscape_dev
{
    public class PlayerDb : DbContext, IPlayerDatabase
    {
        public DbSet<PlayerModel> SaveData { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=data.db");
        }

        protected override void OnModelCreating(ModelBuilder model)
        {
            model.Entity<PlayerModel>(b =>
            {
                b.HasKey(c => c.Username);
                b.Property(s => s.Username).IsRequired();
                b.Property(s => s.PasswordHash).IsRequired();
                b.Property(s => s.X).IsRequired();
                b.Property(s => s.Y).IsRequired();
                b.Property(s => s.Z).IsRequired();
            });
        }

        public Task<PlayerModel> GetPlayer(string username)
        {
            return SaveData.FindAsync(username.ToLowerInvariant());
        }

        public async Task<PlayerModel> GetPlayer(string username, string password)
        {
            var player = await GetPlayer(username);

            if (player == null)
                return null;

            if(!InternalIsValidPwd(player.PasswordHash, password))
                return null;

            return player;
        }

        public async Task Save()
        {
            await SaveChangesAsync();
        }

        public async Task<PlayerModel> CreatePlayer(string username, string password)
        {
            if ((await GetPlayer(username)) != null)
                return null;

            var model = new PlayerModel(username, password);
            SaveData.Add(model);
            await Save();
            return model;
        }

        private bool InternalIsValidPwd(string p1, string p2)
        {
            return p1.Equals(p2, StringComparison.Ordinal);
        }

        public Task<bool> IsValidPassword(string pwd1, string pwd2)
        {
            // NET Core has no bindings for libsodium, so let's just store them in plaintext.
            // TODO: IF YOU ARE DEVELOPING A SERVER FOR PRODUCTION, IMPLEMENT A PASSWORD HASHING SOLUTION
            // todo: check up on https://github.com/jedisct1/libsodium/issues/504 for libsodium bindings
            return Task.FromResult(InternalIsValidPwd(pwd1, pwd2));
        }
    }
}