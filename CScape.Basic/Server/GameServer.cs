using System;
using System.Threading.Tasks;
using CScape.Core;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entity;
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
        public IEntitySystem Entities { get; }

        public bool IsDisposed { get; private set; }
        public DateTime StartTime { get; private set; }

        private ILogger Log { get; }
        private IMainLoop Loop { get; }

        private IPlayerFactory Players { get; }

        public GameServer([NotNull] IServiceCollection services)
        {
            // register internal dependencies
            services.AddSingleton<IGameServer>(_ => this);

            // build service provider
            Services = services?.BuildServiceProvider() ?? throw new ArgumentNullException(nameof(services));

            Overworld = new PlaneOfExistence(this, "Overworld");

            Loop = Services.ThrowOrGet<IMainLoop>();
            Log = Services.ThrowOrGet<ILogger>();;
            Entities = new EntitySystem(this);
            Players = Services.ThrowOrGet<IPlayerFactory>();
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

                // destroy entities
                foreach(var ent in Entities.All.Keys)
                    Entities.Destroy(ent);
                    
                // block as we're saving.
                Services.GetService<IPlayerDatabase>().Save().GetAwaiter().GetResult();

                (Services as IDisposable)?.Dispose();
            }
        }
    }
}
