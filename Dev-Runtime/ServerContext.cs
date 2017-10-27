using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using CScape.Commands;
using CScape.Core;
using CScape.Core.Database;
using CScape.Core.Game;
using CScape.Core.Game.Entity;
using CScape.Core.Log;
using CScape.Core.Network;
using CScape.Models;
using CScape.Models.Game.Command;
using CScape.Models.Game.Entity.Factory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Nito.AsyncEx;

namespace CScape.Dev.Runtime
{
    public class ServerContext
    {
        public GameServer Server { get; private set; }

        private void HandleAggregateException(AggregateException aggEx)
        {
            foreach (var ex in aggEx.InnerExceptions)
                ExceptionDispatchInfo.Capture(ex).Throw();
            // Enable all CLR exceptions in the exception settings window to see the stack-trace.
        }

        public void RunBlocking()
        {
            // make sure we're invariant
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            var dirBuild = Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.Location);

            // make sure we get notified of unobserved task exceptions
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                if (e.Observed)
                    return;
                HandleAggregateException(e.Exception);
                e.SetObserved();
            };
            
            // set up servicess
            var services = new ServiceCollection();

            var cfg = JsonConvert.DeserializeObject<JsonGameServerConfig>(
                File.ReadAllText(
                    Path.Combine(dirBuild, "config.json")));

            services.AddSingleton<SkillDb>(s => new SkillDb(s));
            services.AddSingleton<IPlayerFactory>(s => new PlayerFactory(s.ThrowOrGet<IGameServer>().Entities));
            services.AddSingleton<INpcFactory>(s => new NpcFactory(s.ThrowOrGet<IGameServer>().Entities));
            services.AddSingleton<ItemDatabase>(s => new ItemDatabase());
            services.AddSingleton<IMainLoop>(s => new MainLoop(s));
            services.AddSingleton<ILogger>(s => new Logger(s.ThrowOrGet<IGameServer>()));
            services.AddSingleton<IGameServerConfig>(s => cfg);
            services.AddSingleton<InterfaceIdDatabase>(s => JsonConvert.DeserializeObject<InterfaceIdDatabase>("interface-ids.json"));
            services.AddSingleton<ICommandHandler>(s => new CommandDispatch());
            services.AddSingleton<IPacketParser>(s => new PacketParser(s.ThrowOrGet<IGameServer>().Services));
            services.AddSingleton<IPacketHandlerCatalogue>(s => new PacketHandlerCatalogue(s));
            services.AddSingleton<PlayerJsonDatabase>(s => new PlayerJsonDatabase(s));

            services.AddSingleton<IPacketDatabase>(s => 
                JsonConvert.DeserializeObject<JsonPacketDatabase>(
                        File.ReadAllText(
                            Path.Combine(dirBuild, "packet-lengths.json"))));


            Server = new GameServer(services);

            AsyncContext.Run(async () =>
            {
                await Server.Start().ContinueWith(t =>
                {
                    if (t.Exception != null)
                    {
                        HandleAggregateException(t.Exception);
                    }
                });
            });
        }
    }
}