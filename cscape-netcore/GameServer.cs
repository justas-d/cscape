using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CScape.Game.Entity;
using CScape.Game.Worldspace;
using CScape.Network;
using JetBrains.Annotations;

namespace CScape
{
    public class GameServer
    {
        internal readonly SocketAndPlayerDatabaseDispatch InternalEntry;
        public Logger Log { get; }

        public IGameServerConfig Config { get; }
        public IDatabase Database { get; }

        public DateTime StartTime { get; private set; }

        private MainLoop Loop { get; }
        public PlaneOfExistance Overworld { get; }
        public IdPool EntityIdPool { get; } = new IdPool();
        public AggregateEntityPool<AbstractEntity> Entities { get; } = new AggregateEntityPool<AbstractEntity>();

        public int PlayerCount { get; private set; }

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
            if (!File.Exists(config.PrivateLoginKeyDir)) throw new FileNotFoundException($"Could not find private login key in directory: {config.PrivateLoginKeyDir}");
            Database = database ?? throw new ArgumentNullException(nameof(database));

            Config = config;
            Log = new Logger(this);
            Loop = new MainLoop(this, Config.TickTime);
            InternalEntry = new SocketAndPlayerDatabaseDispatch(this, Loop.LoginQueue);
            Overworld = new PlaneOfExistance(this);

            // todo : take into account Config.MaxPlayers (return status in "QueryServerState" method for login dispatch)?
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

            await Loop.Run();
        }

        public HashSet<Player> Players { get;} = new HashSet<Player>();

        internal void RegisterNewPlayer(Player player)
        {
            Log.Debug(this, $"Registering new player: {player.Username}.");
            if (!Players.Add(player))
                Log.Warning(this, $"Tried to register existing player: {player.Username}.");
        }

        internal void UnregisterPlayer(Player player)
        {
            Log.Debug(this, $"Unregistering player: {player.Username}.");
            if(!Players.Remove(player))
                Log.Warning(this, $"Tried to unregister player that is not registered: {player.Username}");
        }
    }
}
