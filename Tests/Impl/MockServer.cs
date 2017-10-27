using System;
using System.Threading.Tasks;
using CScape.Core.Game.Entity;
using CScape.Core.Game.World;
using CScape.Models;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Factory;
using CScape.Models.Game.World;
using Microsoft.Extensions.DependencyInjection;

namespace CScape.Dev.Tests.Impl
{

    public class MockServer : IGameServer
    {

        public MockServer() : this(new ServiceCollection()) { }

        public MockServer(IServiceCollection services)
        {
            UTCStartTime = DateTime.UtcNow;

            //var dirBuild = Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.Location);

            Entities = new EntitySystem(this);

            var playerFactory = new PlayerFactory(Entities);
            services.AddSingleton(playerFactory);
            services.AddSingleton<IPlayerFactory>(playerFactory);

            services.AddSingleton<IGameServerConfig>(_ => new MockConfig());
            services.AddSingleton<ILogger>(_ => new TestLogger());
            services.AddSingleton<IGameServer>(_ => this);
            Loop = new MockLoop(this);
            services.AddSingleton<IMainLoop>(s => Loop);
            
            Services = services.BuildServiceProvider();


            Overworld = new PlaneOfExistence(this, "Mock overworld");
        }

        public void Dispose()
        {
            
        }

        public IServiceProvider Services { get; }
        public IPlaneOfExistence Overworld { get; }
        public IMainLoop Loop { get; }
        public IEntitySystem Entities { get; }

        public bool IsDisposed => false;
        public DateTime UTCStartTime { get; }
        public ServerStateFlags GetState() => 0;
        public Task Start() => null;
    }
}