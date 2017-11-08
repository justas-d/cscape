using CScape.Core;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity;
using CScape.Core.Json;
using CScape.Models;
using CScape.Models.Data;
using CScape.Models.Game.Entity;
using Microsoft.Extensions.DependencyInjection;

namespace CScape.Dev.Tests.Mock
{
    public class CoreModelImpl : IModelImplementation
    {
        public IEntityHandle CreateEntity(string name)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IEntitySystem>(s => new EntitySystem(s));
            services.AddSingleton<ILogger>(new TestLogger());
            services.AddSingleton<IConfigurationService>(s => new JsonConfigurationService(s, "config.json"));

            var server = new GameServer(services);
            return server.Services.ThrowOrGet<IEntitySystem>().Create(name);
        }

        public IConfigurationService GetConfig()
        {
            var builder = new ServiceCollection();
            builder.AddSingleton<ILogger>(new TestLogger());
            builder.AddSingleton<IConfigurationService>(s => new InMemoryConfigurationService(s));

            var services = builder.BuildServiceProvider();
            return services.ThrowOrGet<IConfigurationService>();
        }
    }
}