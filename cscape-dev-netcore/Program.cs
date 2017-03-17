using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using cscape;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace cscape_dev
{
    public class SaveData : IPlayerSaveData
    {
        public int Id { get; set; }
        public string PasswordHash { get; set; }
        public string Username { get; set; }
        public byte TitleIcon { get; set; }

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
        }

    }

    public class ServerDatabase : IDatabase, IDisposable
    {
        public IPacketLengthLookup Packet { get; }
        public IPlayerDatabase Player => _playerDb;

        private PlayerDb _playerDb;

        public ServerDatabase(string sqliteDbDir, string packetJsonDir)
        {
            Packet = JsonConvert.DeserializeObject<PacketLookup>(File.ReadAllText(packetJsonDir));
            _playerDb = new PlayerDb();
            _playerDb.Database.Migrate();
        }

        public void Dispose()
        {
            _playerDb?.Dispose();
            _playerDb = null;
        }
    }

    public class PlayerDb : DbContext, IPlayerDatabase
    {
        public DbSet<SaveData> SaveData { get; set; } 

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=data.db");
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

            var data = await SaveData.FirstAsync(s => s.Username == username);
            if (!await IsValidPassword(data.PasswordHash, password))
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
            SaveData.Add(data);
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
                TitleIcon = 0
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

    static class Program
    {
        private static readonly BlockingCollection<LogEventArgs> LogQueue = new BlockingCollection<LogEventArgs>();
        private static GameServer _server;

        private static void HandleAggregateException(AggregateException aggEx)
        {
            foreach (var ex in aggEx.InnerExceptions)
                ExceptionDispatchInfo.Capture(ex).Throw();
            // Enable all CLR exceptions in the exception settings window to see the stack-trace.
        }

        static void Main()
        {
            // make sure we're invariant
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            // config
            var cfg = JsonConvert.DeserializeObject<JsonGameServerConfig>(File.ReadAllText("config.json"));
            _server = new GameServer(cfg, new ServerDatabase("data.db", "packet-lengths.json"));

            _server.Log.LogReceived += (s, l) => LogQueue.Add(l);

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                if (e.Observed)
                    return;
                HandleAggregateException(e.Exception);
                e.SetObserved();
            };

            ThreadPool.QueueUserWorkItem(o =>
            {
                foreach (var log in LogQueue.GetConsumingEnumerable())
                    WriteLog(log);
            });

            Task.Run(_server.Start).ContinueWith(t =>
            {
                if (t.IsFaulted && t.Exception != null)
                    HandleAggregateException(t.Exception);
            });

            while (true)
                Console.ReadLine();
        }

        private static void WriteIntoDelegate(Action<string> writeDel, LogEventArgs l, double sec)
        {
            string header = $"[{sec,4:N6}] ";
            writeDel(header + l.Message);
            if (l.Exception != null)
            {
                writeDel(Environment.NewLine);
                writeDel(new string(' ', header.Length + 1) + $"-> Exception: {l.Exception}");
            }
            writeDel(Environment.NewLine);

        }

        private static void WriteLog(LogEventArgs log)
        {
            var time = log.Time - _server.StartTime;
#if DEBUG
            if (log.Severity == LogSeverity.Debug)
                WriteIntoDelegate(w => Debug.Write(w), log, time.TotalSeconds);
            else
#endif
                WriteIntoDelegate(Console.Write, log, time.TotalSeconds);
        }
    }
}