using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using cscape;
using Newtonsoft.Json;
using Sodium;
using SQLite;

namespace cscape_dev
{
    class ServerDatabase : IDatabase
    {
        public IPacketLengthLookup Packet { get; }
        public IPlayerDatabase Player { get; }

        private SQLiteAsyncConnection _db;

        public ServerDatabase(string sqliteDbDir, string packetJsonDir)
        {
            _db = new SQLiteAsyncConnection(sqliteDbDir);
            Packet = JsonConvert.DeserializeObject<PacketLookup>(File.ReadAllText(packetJsonDir));

            Player = new PlayerDb(_db);
        }
    }

    class PlayerDb : IPlayerDatabase
    {
        private class SaveData : IPlayerSaveData
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; }
            public string PasswordHash { get; }
            [MaxLength(Player.MaxUsernameChars), Indexed]
            public string Username { get; }
            public byte TitleIcon { get; }

            /// <summary>
            /// SQLite constructor
            /// </summary>
            public SaveData()
            {
                
            }

            /// <summary>
            /// New player constructor
            /// </summary>
            public SaveData(string username, string pwdHash)
            {
                Username = username;
                PasswordHash = pwdHash;
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

        private readonly SQLiteAsyncConnection _db;

        private async Task<SaveData> GetUser(string username)
        {
            return await _db.Table<SaveData>()
                    .Where(u => string.Equals(username, u.Username, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefaultAsync();
        }

        public PlayerDb(SQLiteAsyncConnection db)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            _db = db;
            _db.CreateTableAsync<SaveData>();
        }

        public async Task<IPlayerSaveData> Load(string username, string password)
        {
            var user = await GetUser(username);
            if (user == null)
                return null;

            if (!await Task.Run(() => PasswordHash.ScryptHashStringVerify(user.PasswordHash, password)))
                return null;

            return user;
        }

        public Task Save(Player player)
        {
            var data = new SaveData(player);
            return Save(data);
        }

        private async Task Save(SaveData save)
        {
            //@TODO: test db saving
            if ((await GetUser(save.Username)) == null)
                await _db.InsertAsync(save);
            else
                await _db.UpdateAsync(save);
        }

        public async Task<bool> UserExists(string username) => await GetUser(username) != null;

        public async Task<IPlayerSaveData> LoadOrCreateNew(string username, string pwd)
        {
            if (await UserExists(username))
                return await Load(username, pwd);

            // @todo: default stats
            var data = new SaveData(
                username, 
                await Task.Run(() => PasswordHash.ScryptHashString(pwd, PasswordHash.Strength.Medium)));

            await Save(data);
            return data;
        }
    }

    static class Program
    {
        private static readonly BlockingCollection<LogEventArgs> LogQueue = new BlockingCollection<LogEventArgs>();
        private static GameServer _server;

        static void Main()
        {
            // config
            var cfg = JsonConvert.DeserializeObject<JsonGameServerConfig>(File.ReadAllText("config.json"));
            _server = new GameServer(cfg, new ServerDatabase("data.db", "packet-lengths.json"));

            _server.Log.LogReceived += (s, l) => LogQueue.Add(l);

            ThreadPool.QueueUserWorkItem(o =>
            {
                foreach (var log in LogQueue.GetConsumingEnumerable())
                    WriteLog(log);
            });

            _server.Start();

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