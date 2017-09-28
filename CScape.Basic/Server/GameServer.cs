using System;
using System.Threading.Tasks;
using CScape.Core;
using CScape.Core.Game.Entity;
using CScape.Core.Game.NewEntity;
using CScape.Core.Game.World;
using CScape.Core.Injection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace CScape.Basic.Server
{
    public sealed class GameServer : IGameServer
    {
        public IServiceProvider Services { get; }
        public PlaneOfExistence Overworld { get; }

        public bool IsDisposed { get; private set; }
        public DateTime StartTime { get; private set; }

        private ILogger Log { get; }
        private IMainLoop Loop { get; }

        public GameServer([NotNull] IServiceCollection services)
        {
            // register internal dependencies
            services.AddSingleton<IGameServer>(_ => this);

            // build service provider
            Services = services?.BuildServiceProvider() ?? throw new ArgumentNullException(nameof(services));

            Overworld = new PlaneOfExistence(this, "Overworld");

            Loop = Services.ThrowOrGet<IMainLoop>();
            Log = Services.ThrowOrGet<ILogger>();
            Services.ThrowOrGet<IEntitySystem>();
        }

        public ServerStateFlags GetState()
        {
            ServerStateFlags ret = 0;

            if (Players.All.Count >= Services.GetService<IGameServerConfig>().MaxPlayers)
                ret |= ServerStateFlags.PlayersFull;

            if (!Services.GetService<ILoginService>().IsEnabled)
                ret |= ServerStateFlags.LoginDisabled;

            return ret;
        }

        public async Task Start()
        {
            StartTime = DateTime.Now;

            Log.Normal(this, "Starting server...");

            await Loop.Run();
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;

                // block as we're saving.
                Services.GetService<IPlayerDatabase>().Save().GetAwaiter().GetResult();

                foreach (var p in Players.All.Values)
                    p.Connection.Dispose();

                (Services as IDisposable)?.Dispose();
            }
        }
    }
}
