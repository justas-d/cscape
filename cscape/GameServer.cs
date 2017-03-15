using System;
using System.Collections.Generic;
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
            if (config.PrivateLoginKeyDir == null) throw new ArgumentNullException(nameof(config.PrivateLoginKeyDir));
            if (config.ListenEndPoint == null) throw new ArgumentNullException(nameof(config.ListenEndPoint));
            if (config.MaxPlayers <= 0) throw new ArgumentOutOfRangeException(nameof(config.MaxPlayers));
            if (config.Backlog <= 0) throw new ArgumentOutOfRangeException(nameof(config.Backlog));

            if (!File.Exists(config.PrivateLoginKeyDir))
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

            //todo run the entry point task with a cancellation token
#pragma warning disable 4014
            Task.Run(_entry.StartListening).ContinueWith(t =>
#pragma warning restore 4014
            {
                Log.Debug(this, $"EntryPoint listen task terminated in status: Completed: {t.IsCompleted} Faulted: {t.IsFaulted} Cancelled: {t.IsCanceled}");
                if (t.Exception != null)
                    Log.Exception(this, "EntryPoint contained exception.", t.Exception);
            });

            Log.Normal(this, "Server live.");

            //TODO: bool to terminate main loop

            const int tickMs = 600;
            var watch = new Stopwatch();
            var waitTime = 0;
            var playerRemoveQueue = new Queue<int>();
            while (true)
            {
                // ====== PROLOGUE ====== 

                watch.Start();

                // ====== BODY ====== 

                IPlayerLogin login;
                while (_entry.LoginQueue.TryDequeue(out login))
                    login.Transfer(Players);
               
                foreach (var player in Players)
                {
                    if (player.Connection.ManageHardDisconnect(waitTime + watch.ElapsedMilliseconds))
                        playerRemoveQueue.Enqueue(player.InstanceId);

                    if (player.Connection.IsConnected())
                    {
                        // todo : exception handle synchronization
                        foreach (var sync in player.SyncMachines)
                            await sync.Synchronize(player.Connection.Socket);
                    }

                    // todo : packet handling, player io
                }

                // ====== EPILOGUE ====== 
                if (playerRemoveQueue.Count > 0)
                {
                    Log.Debug(this, $"Reaping {playerRemoveQueue.Count} players.");
                    while (playerRemoveQueue.Count > 0)
                        Players.Remove(playerRemoveQueue.Dequeue());
                }

                watch.Stop();
                waitTime = (int)(tickMs - watch.ElapsedMilliseconds);
                watch.Reset();
                if (waitTime <= 0)
                {
                    Log.Warning(this, $"Tick process time too slow! need to wait for {waitTime}ms. Tick target ms: {tickMs}ms.");
                    continue;
                }
                await Task.Delay(waitTime);
            }
        }
    }
}
