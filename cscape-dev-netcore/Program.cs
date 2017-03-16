using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using cscape;
using Newtonsoft.Json;

namespace cscape_dev
{
    class ServerDatabase : IDatabase
    {
        public IPacketLengthLookup Packet { get; }
        public IPlayerDatabase Player { get; }

        public ServerDatabase(string sqliteDbDir, string packetJsonDir)
        {
            Packet = JsonConvert.DeserializeObject<PacketLookup>(File.ReadAllText(packetJsonDir));
        }
    }

    class PlayerDb : IPlayerDatabase
    {
        class SaveData : IPlayerSaveData
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

        public Task<bool> UserExists(string username)
        {
            throw new NotImplementedException();
        }

        public Task<IPlayerSaveData> Load(string username, string password)
        {
            throw new NotImplementedException();
        }

        public Task Save(Player player)
        {
            throw new NotImplementedException();
        }

        public Task<IPlayerSaveData> LoadOrCreateNew(string username, string pwd)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsValidPassword(string pwdHash, string pwd)
        {
            throw new NotImplementedException();
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
        }

        static void Main()
        {
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