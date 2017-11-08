using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Factory;
using CScape.Core.Game.World;
using CScape.Core.Json;
using CScape.Core.Network;
using CScape.Core.Utility;
using CScape.Models;
using CScape.Models.Data;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Factory;
using CScape.Models.Game.World;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace CScape.Core
{
    public sealed class GameServer : IGameServer
    {
        private Lazy<IPlayerCatalogue> _players;
        private Lazy<IConfigurationService> _config;
        private Lazy<SocketAndPlayerDatabaseDispatch> _dispatch;
        private Lazy<PlayerJsonDatabase> _jsonDb;
        private Lazy<ILogger> _log;

        private IPlayerCatalogue PlayerCatalogue => _players.Value;
        private IConfigurationService Config => _config.Value;
        private SocketAndPlayerDatabaseDispatch Dispatch => _dispatch.Value;
        private PlayerJsonDatabase JsonDb => _jsonDb.Value;
        private ILogger Log => _log.Value;

        public IServiceProvider Services { get; }
        public IPlaneOfExistence Overworld { get; }

        public bool IsDisposed { get; private set; }
        public DateTime UTCStartTime { get; private set; }

        public GameServer([NotNull] IServiceCollection services)
        {
            // register internal dependencies
            services.AddSingleton<IGameServer>(_ => this);
            services.AddSingleton(s => new SocketAndPlayerDatabaseDispatch(s.ThrowOrGet<IGameServer>().Services));

            // build service provider
            Services = services.BuildServiceProvider() ?? throw new ArgumentNullException(nameof(services));

            Overworld = new PlaneOfExistence(this, "Overworld");

            _players = Services.GetLazy<IPlayerCatalogue>();
            _config = Services.GetLazy<IConfigurationService>();
            _dispatch = Services.GetLazy<SocketAndPlayerDatabaseDispatch>();
            _jsonDb = Services.GetLazy<PlayerJsonDatabase>();
            _log = Services.GetLazy<ILogger>();
        }

        public ServerStateFlags GetState()
        {
            ServerStateFlags ret = 0;

            if (PlayerCatalogue.NumAlivePlayers >= Config.GetInt(ConfigKey.MaxPlayers))
                ret |= ServerStateFlags.PlayersFull;

            if (!Dispatch.IsEnabled)
                ret |= ServerStateFlags.LoginDisabled;

            return ret;
        }

        public async Task Start(CancellationToken ct)
        {
            UTCStartTime = DateTime.UtcNow;

            Log.Normal(this, "Starting server...");

            await Services.ThrowOrGet<IMainLoop>().Run(ct);
        }

        public void SaveAllPlayers()
        {
            Log.Normal(this, $"Saving players.");

            foreach (var p in PlayerCatalogue.All.Where(p => !p.IsDead()).Select(p => p.Get()))
                JsonDb.Save(p.AssertGetPlayer());
        }
    
        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;

                SaveAllPlayers();

                var entities = Services.GetService<IEntitySystem>();
                entities?.DestroyAll();
            
                (Services as IDisposable)?.Dispose();
            }
        }
    }
}
