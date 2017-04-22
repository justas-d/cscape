using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CScape.Game.Entity;
using CScape.Game.Worldspace;
using CScape.Network;
using CScape.Network.Packet;
using JetBrains.Annotations;

namespace CScape
{
    public class GameServer
    {
        internal readonly SocketAndPlayerDatabaseDispatch InternalEntry;
        public Logger Log { get; }

        /// <summary>
        /// A pool of players currently playing.
        /// </summary>
        public EntityPool<Player> Players { get; }
        public IGameServerConfig Config { get; }
        public IDatabase Database { get; }

        public DateTime StartTime { get; private set; }
        public PacketDispatch PacketDispatch { get; }

        public PlaneOfExistance Overworld { get; }
        public IdPool EntityIdPool { get; } = new IdPool();

        /// <exception cref="ArgumentNullException"><paramref name="config.ListenEndPoint"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="config.PrivateLoginKeyDir"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="config.Version"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="database"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="config.MaxPlayers"/> is less-or-equals to zero</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="config.Backlog"/> is less-or-equals to zero</exception>
        /// <exception cref="FileNotFoundException">Condition.</exception>
        public GameServer([NotNull] IGameServerConfig config, [NotNull] IDatabase database)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            // verify config
            if (config.Version == null) throw new ArgumentNullException(nameof(config.Version));
            if (config.PrivateLoginKeyDir == null) throw new ArgumentNullException(nameof(config.PrivateLoginKeyDir));
            if (config.ListenEndPoint == null) throw new ArgumentNullException(nameof(config.ListenEndPoint));
            if (config.MaxPlayers <= 0) throw new ArgumentOutOfRangeException(nameof(config.MaxPlayers));
            if (config.Backlog <= 0) throw new ArgumentOutOfRangeException(nameof(config.Backlog));

            if (!File.Exists(config.PrivateLoginKeyDir))
                throw new FileNotFoundException($"Could not find private login key in directory: {config.PrivateLoginKeyDir}");

            Database = database ?? throw new ArgumentNullException(nameof(database));
            Config = config;

            Log = new Logger(this);
            PacketDispatch = new PacketDispatch(this);
            InternalEntry = new SocketAndPlayerDatabaseDispatch(this);

            Overworld = new PlaneOfExistance(this);
            Players = new EntityPool<Player>(config.MaxPlayers);
        }

        public async Task Start()
        {
            StartTime = DateTime.Now;

            Log.Normal(this, "Starting server...");

            //todo run the entry point task with a cancellation token
#pragma warning disable 4014
            Task.Run(InternalEntry.StartListening).ContinueWith(t =>
            {
                Log.Debug(this, $"EntryPoint listen task terminated in status: Completed: {t.IsCompleted} Faulted: {t.IsFaulted} Cancelled: {t.IsCanceled}");
                if (t.Exception != null)
                    Log.Exception(this, "EntryPoint contained exception.", t.Exception);
            });

            Log.Normal(this, "Server live.");

            //TODO: bool to terminate main loop
            // todo : exception handle all over the main loop

            const int tickMs = 600;
            var watch = new Stopwatch();
            var deltaTime = 0;
            var waitTime = 0;
            bool overtime;
            var playerRemoveQueue = new Queue<uint>();
            while (true)
            {
                // ====== PROLOGUE ====== 

                watch.Start();

                // ====== BODY ====== 

                // handle new logins
                while (InternalEntry.LoginQueue.TryDequeue(out IPlayerLogin login))
                {
                    var player = login.Transfer(Players);

                    // reconnecting transfers can fail, returning a null.
                    if (player == null)
                        continue;

                    // add the player itself their world
                    player.PoE.AddEntity(player);

                    // send them the initial observables
                    foreach (var entity in player.PoE)
                        player.Observatory.PushObservable(entity);
                }

                // Player network management, syncing, packet reading and parsing loop.
                foreach (var player in Players)
                {
                    if (player.Connection.ManageHardDisconnect(deltaTime + watch.ElapsedMilliseconds))
                        playerRemoveQueue.Enqueue(player.UniqueEntityId);

                    if (player.Connection.IsConnected())
                    {
                        foreach (var sync in player.Connection.SyncMachines)
                            sync.Synchronize(player.Connection.OutStream);

                        // get their data
                        player.Connection.FlushInput();

                        // parse their data
                        foreach (var p in PacketParser.Parse(this, player.Connection.InCircularStream))
                            PacketDispatch.Handle(player, p.Opcode, p.Packet);

                        // send our data
                        player.Connection.SendOutStream();

                        // if the logoff flag is set, log the player off.
                        if (player.LogoffFlag)
                        {
                            player.Connection.Dispose(); // shut down the connection
                            player.Save(); // queue save
                            playerRemoveQueue.Enqueue(player.UniqueEntityId);
                        }
                    }
                }

                // ====== EPILOGUE ====== 

                // clean dead players
                if (playerRemoveQueue.Count > 0)
                {
                    Log.Debug(this, $"Reaping {playerRemoveQueue.Count} players.");
                    while (playerRemoveQueue.Count > 0)
                        Players.Remove(playerRemoveQueue.Dequeue());
                }

                // handle tick delays
                watch.Stop();
                watch.Reset();

                waitTime = Math.Abs(tickMs - (int)watch.ElapsedMilliseconds);
                overtime = waitTime < tickMs;
                if (overtime)
                    Log.Warning(this, $"Tick process time too slow! need to wait for {waitTime}ms. Tick target ms: {tickMs}ms.");
                else
                    await Task.Delay(waitTime);

                deltaTime = overtime ? tickMs : waitTime;
            }
        }
    }
}
