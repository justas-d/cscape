using System;
using System.Threading;
using System.Threading.Tasks;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Entity.Factory;
using CScape.Core.Game.Skill;
using CScape.Core.Game.World;
using CScape.Core.Json.Model;
using CScape.Models;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.World;
using Microsoft.Extensions.DependencyInjection;

namespace CScape.Core.Tests.Impl
{

    public class MockServer : IGameServer
    {

        public MockServer() : this(new ServiceCollection()) { }

        public MockServer(IServiceCollection services)
        {
            UTCStartTime = DateTime.UtcNow;

            services.AddSingleton(s => new EntitySystem(s));
            services.AddSingleton<IEntitySystem>(s => s.ThrowOrGet<EntitySystem>());

            services.AddSingleton(s => new PlayerCatalogue(s));
            services.AddSingleton<IPlayerCatalogue>(s => s.ThrowOrGet<PlayerCatalogue>());

            services.AddSingleton(s => new NpcFactory(s));
            services.AddSingleton<INpcFactory>(s => s.ThrowOrGet<NpcFactory>());

            services.AddSingleton<ILogger>(_ => new TestLogger());
            services.AddSingleton<IGameServer>(_ => this);
            services.AddSingleton<IInterfaceIdDatabase>(new MockInterfaceDb());
            services.AddSingleton<SkillDb>();
            services.AddSingleton<IMainLoop>(s => new MockLoop(this));
        
            Services = services.BuildServiceProvider();

            Overworld = new PlaneOfExistence(this, "Mock overworld");
        }

        public void Dispose()
        {
            
        }

        public IServiceProvider Services { get; }
        public IPlaneOfExistence Overworld { get; }

        public bool IsDisposed => false;
        public DateTime UTCStartTime { get; }
        public ServerStateFlags GetState() => 0;
        public Task Start(CancellationToken token) => null;
    }
}