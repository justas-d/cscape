using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using CScape.Game.Entity;
using CScape.Game.World;
using CScape.Network;
using JetBrains.Annotations;

namespace CScape
{
    public class GameServer
    {
        private readonly SocketAndPlayerDatabaseDispatch _entry;
        public Logger Log { get; }

        public IGameServerConfig Config { get; }
        public IDatabase Database { get; }

        public DateTime StartTime { get; private set; }

        internal MainLoop Loop { get; }
        public PlaneOfExistance Overworld { get; }
        public IdPool EntityIdPool { get; } = new IdPool();
        public IdPool PlayerIdPool { get; }
        public AggregateEntityPool<IWorldEntity> Entities { get; } 
            = new AggregateEntityPool<IWorldEntity>();

        public ImmutableDictionary<int, Player> Players { get; private set; } = ImmutableDictionary<int, Player>.Empty;

        public bool IsLoginEnbled { get; set; } = true;

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
            _entry = new SocketAndPlayerDatabaseDispatch(this, Loop.LoginQueue);
            Overworld = new PlaneOfExistance(this);
            PlayerIdPool = new IdPool(Convert.ToUInt32(config.MaxPlayers));
        }

        [Flags]
        public enum ServerStateFlags
        {
            None,
            PlayersFull,
            LoginDisabled
        }

        public ServerStateFlags GetState()
        {
            ServerStateFlags ret = 0;
            if (Players.Count >= Config.MaxPlayers)
                ret |= ServerStateFlags.PlayersFull;
            if(!IsLoginEnbled)
                ret |= ServerStateFlags.LoginDisabled;
            return ret;
        }

        public async Task Start()
        {
            StartTime = DateTime.Now;

            Log.Normal(this, "Starting server...");

            //todo run the entry point task with a cancellation token
#pragma warning disable 4014
            Task.Run(_entry.StartListening).ContinueWith(t =>
            {
                Log.Debug(this, $"EntryPoint listen task terminated in status: Completed: {t.IsCompleted} Faulted: {t.IsFaulted} Cancelled: {t.IsCanceled}");
                if (t.Exception != null)
                    Log.Exception(this, "EntryPoint contained exception.", t.Exception);
            });

            await Loop.Run();
        }

        public void SavePlayers()
            => _entry.SaveFlag = true;

        [CanBeNull]
        public Player GetPlayerByPid(int pid)
        {
            if (!Players.ContainsKey(pid))
            {
                Log.Warning(this, $"Attempted to get unregistered pid: {pid}");
                return null;
            }
            
            return Players[pid];
        }

        internal void RegisterNewPlayer(Player player)
        {
            Log.Debug(this, $"Registering new player: {player.Username}.");

            if (Players.ContainsKey(player.Pid))
            {
                Log.Warning(this, $"Tried to register existing player: {player.Username}.");
                return;
            }

            Players = Players.Add(player.Pid, player);
        }

        internal void UnregisterPlayer(Player player)
        {
            Log.Debug(this, $"Unregistering player: {player.Username}.");

            if (!Players.ContainsKey(player.Pid))
            {
                Log.Warning(this, $"Tried to unregister player that is not registered: {player.Username}");
                return;
            }

            Players = Players.Remove(player.Pid);
        }
    }
}
