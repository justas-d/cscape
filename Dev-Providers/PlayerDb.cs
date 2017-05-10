using System;
using System.Threading.Tasks;
using CScape.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CScape.Dev.Providers
{
    public class PlayerDb : DbContext, IPlayerDatabase
    {
        public DbSet<PlayerModel> PlayerModels { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=data.db");
        }

        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);

            void SetupForeign<T>(ReferenceNavigationBuilder<PlayerModel, T> b) where T : class, IForeignModelObject<string, PlayerModel>
            {
                b.WithOne(c => c.Model)
                    .HasForeignKey<T>(p => p.ForeignKey)
                    .IsRequired();
            }

            model.Entity<PlayerAppearance>(b =>
            {
                b.HasKey(p => p.ForeignKey);
            });

            model.Entity<ItemProviderModel>(b =>
            {
                b.HasKey(m => m.ForeignKey);
                b.Property(m => m.Ids).IsRequired();
                b.Property(m => m.Amounts).IsRequired();
                b.Property(m => m.Size).IsRequired();
            });

            model.Entity<PlayerModel>(b =>
            {
                b.HasKey(c => c.Username);
                b.Property(s => s.Username).IsRequired();
                b.Property(s => s.PasswordHash).IsRequired();
                b.Property(s => s.X).IsRequired();
                b.Property(s => s.Y).IsRequired();
                b.Property(s => s.Z).IsRequired();
                
                SetupForeign(b.HasOne(p => p.BackpackItems));
                SetupForeign(b.HasOne(p => p.Appearance));
            });
        }

        public Task<PlayerModel> GetPlayer(string username)
        {
            return PlayerModels.FindAsync(username.ToLowerInvariant());
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
            PlayerModels.Add(model);
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