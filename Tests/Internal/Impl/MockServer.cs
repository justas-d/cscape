using System;
using System.Threading.Tasks;
using CScape.Basic.Database;
using CScape.Basic.Model;
using CScape.Basic.Server;
using CScape.Core;
using CScape.Core.Game.Entity;
using CScape.Core.Game.World;
using CScape.Core.Injection;
using Microsoft.Extensions.DependencyInjection;

namespace CScape.Dev.Tests.Internal.Impl
{
    public class MockServer : IGameServer
    {
        public AggregateEntityPool<IWorldEntity> Entities { get; } = new AggregateEntityPool<IWorldEntity>();
        public IEntityRegistry<short, Player> Players { get; }
        public IEntityRegistry<int, Npc> Npcs { get; }

        public void Dispose() { }

        public IServiceProvider Services { get; }
        public PlaneOfExistance Overworld { get; }

        public bool IsDisposed { get; }
        public DateTime StartTime { get; }

        public MockServer()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IItemDefinitionDatabase>(_ => new MockItemDb());
            services.AddSingleton<IInterfaceIdDatabase>(_ => InterfaceDb.FromJson("mock-interface-ids.json"));
            services.AddSingleton<IMainLoop>(_ => new MockLoop());
            services.AddSingleton<IIdPool>(_ => new IdPool());
            services.AddSingleton<ILogger>(_ => new TestLogger());
            services.AddSingleton<IGameServer>(_ => this);
            Services = services.BuildServiceProvider();

            Overworld = new PlaneOfExistance(this, "Mock overworld");

            Players = new PlayerRegistry(Services);
            Npcs = new NpcRegistry(Services);
        }

        public ServerStateFlags GetState() => 0;
        public Task Start() => null;
    }
}