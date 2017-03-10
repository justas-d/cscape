using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace cscape
{
    public class GameServer
    {
        private readonly PlayerEntryPoint _entry;
        public Logger Log { get; }

        /// <summary>
        /// A pool of players currently playing.
        /// </summary>
        public EntityPool<Player> Players { get; }
        public IGameServerConfig Config { get; }
        public IDatabase Database { get; }

        public DateTime StartTime { get; private set; }

        public GameServer([NotNull] IGameServerConfig config, [NotNull] IDatabase database)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (database == null) throw new ArgumentNullException(nameof(database));

            // verify config
            if (config.Version == null) throw new ArgumentNullException(nameof(config.Version));
            if (config.PrivateLoginKeyDir== null) throw new ArgumentNullException(nameof(config.PrivateLoginKeyDir));
            if (config.ListenEndPoint== null) throw new ArgumentNullException(nameof(config.ListenEndPoint));
            if (config.MaxPlayers<= 0) throw new ArgumentOutOfRangeException(nameof(config.MaxPlayers));
            if (config.Backlog<= 0) throw new ArgumentOutOfRangeException(nameof(config.Backlog));

            if(!File.Exists(config.PrivateLoginKeyDir))
                throw new FileNotFoundException($"Could not find private login key in directory: {config.PrivateLoginKeyDir}");

            Config = config;
            Database = database;

            Log = new Logger(this);
            _entry = new PlayerEntryPoint(this);
            Players = new EntityPool<Player>(config.MaxPlayers);
        }

        public async Task Start()
        {
            StartTime = DateTime.Now;

            Log.Normal(this, "Starting server...");

            //@TODO run the entry point task with a cancellation token
            Task.Run(_entry.StartListening).ContinueWith(t =>
            {
                Log.Debug(this, $"EntryPoint listen task terminated in status: Completed: {t.IsCompleted} Faulted: {t.IsFaulted} Cancelled: {t.IsCanceled}");
                if (t.Exception != null)
                    Log.Exception(this, "EntryPoint contained exception.", t.Exception);
            });

            Log.Normal(this, "Server live.");

            //TODO: bool to terminate mainloop
            var watch = new Stopwatch();
            const int tickMs = 600;
            while (true)
            {
                watch.Start();
                // todo : packet handling, player io, game ticks
                Player newPlayer;
                while (!_entry.PlayerQueue.TryDequeue(out newPlayer))
                {
                    Players.Add(newPlayer);
                }


                watch.Stop();

                var delta = (int)(tickMs - watch.ElapsedMilliseconds);
                if (delta <= 0)
                {
                    Log.Warning(this, $"Tick process time too slow! need to wait for {delta}ms. Tick target ms: {tickMs}ms, watch time: {watch.ElapsedMilliseconds}ms");
                    continue;
                }

                await Task.Delay(delta);
            }
        }
    }
}
