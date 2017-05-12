using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CScape.Basic.Model;
using CScape.Core.Injection;
using Microsoft.EntityFrameworkCore;

namespace CScape.Basic.Database
{
    public class PlayerDatabase : DbContext, IPlayerDatabase
    {
        public DbSet<PlayerModel> PlayerModels { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=data.db");
        }

        protected override void OnModelCreating(ModelBuilder model)
        {
            void RegisterPlayerLeaf<T>(Expression<Func<PlayerModel, T>> modelToLeaf) where T : class, IDbPlayerLeaf
            {
                model.Entity<PlayerModel>()
                    .HasOne(modelToLeaf)
                    .WithOne(m => m.Player)
                    .HasForeignKey<T>(m => m.PlayerId);
                model.Entity<T>().Property(m => m.Id).ValueGeneratedOnAdd();
            }

            RegisterPlayerLeaf(m => m.BackpackItems);
            RegisterPlayerLeaf(m => m.Appearance);

            model.Entity<PlayerModel>(b =>
            {
                b.Property(m => m.TitleIcon).IsRequired();
                b.Property(m => m.PasswordHash).IsRequired();
                b.Property(m => m.IsMember).IsRequired();

                b.Property(m => m.X).IsRequired();
                b.Property(m => m.Y).IsRequired();
                b.Property(m => m.Z).IsRequired();
            });

            model.Entity<ItemProviderModel>(b =>
            {
                b.Property(m => m.Size).IsRequired();

                b.Ignore(m => m.Ids);
                b.Ignore(m => m.Amounts);

                b.Property(m => m.DbIds).IsRequired();
                b.Property(m => m.DbAmounts).IsRequired();
            });
        }

        public async Task<IPlayerModel> GetPlayer(string username)
        {
            username = username.ToLowerInvariant();
            
            // todo : merge RegisterPlayerLeaf and .Include calls in GetPlayer
            return await PlayerModels
                .Include(m => m.BackpackItems)
                .Include(m => m.Appearance)
                .FirstOrDefaultAsync(f => f.Id == username);
        }

        public async Task<IPlayerModel> GetPlayer(string username, string password)
        {
            var player = await GetPlayer(username);

            if (player == null)
                return null;

            if(!InternalIsValidPwd(player.PasswordHash, password))
                return null;

            return player;
        }

        public async Task Save() => await SaveChangesAsync();

        public async Task<IPlayerModel> CreatePlayer(string username, string password)
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