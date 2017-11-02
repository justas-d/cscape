using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Loader;
using System.Threading;
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
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Factory;
using CScape.Models.Game.Item;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Nito.AsyncEx;

namespace CScape.Dev.Runtime
{
    public class ServerContext : IDisposable
    {
        private GameServer _server;
        private CancellationTokenSource _cts = new CancellationTokenSource();

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

            services.AddSingleton(s => new PlayerFactory(s));
            services.AddSingleton<IPlayerFactory>(s => s.ThrowOrGet<PlayerFactory>());

            services.AddSingleton(s => new NpcFactory(s));
            services.AddSingleton<INpcFactory>(s => s.ThrowOrGet<NpcFactory>());

            services.AddSingleton(s => new MainLoop(s));
            services.AddSingleton<IMainLoop>(s => s.ThrowOrGet<MainLoop>());

            services.AddSingleton(s => new GroundItemFactory(s.ThrowOrGet<IEntitySystem>()));
            services.AddSingleton<IGroundItemFactory>(s => s.ThrowOrGet<GroundItemFactory>());

            services.AddSingleton(s => new EntitySystem(s.ThrowOrGet<IGameServer>()));
            services.AddSingleton<IEntitySystem>(s => s.ThrowOrGet<EntitySystem>());

            services.AddSingleton<ItemDatabase>(s => new ItemDatabase());
            services.AddSingleton<ILogger>(s => new Logger(s.ThrowOrGet<IGameServer>()));
            services.AddSingleton<IGameServerConfig>(s => cfg);
            services.AddSingleton<ICommandHandler>(s =>
            {
                var a = new CommandDispatch();
                a.RegisterAssembly(typeof(Entity).GetTypeInfo().Assembly);
                return a;
            });


            services.AddSingleton(s =>
                InterfaceIdDatabase.FromJson(Path.Combine(Core.Utils.GetExeDir(), "interface-ids.json")));
            services.AddSingleton<IInterfaceIdDatabase>(s => s.ThrowOrGet<InterfaceIdDatabase>());

            services.AddSingleton<ICommandHandler>(s => new CommandDispatch());
            services.AddSingleton<IPacketParser>(s => new PacketParser(s.ThrowOrGet<IGameServer>().Services));
            services.AddSingleton<IPacketHandlerCatalogue>(s => new PacketHandlerCatalogue(s));
            services.AddSingleton<PlayerJsonDatabase>(s => new PlayerJsonDatabase(s));

            services.AddSingleton<IPacketDatabase>(s =>
                JsonConvert.DeserializeObject<JsonPacketDatabase>(
                    File.ReadAllText(
                        Path.Combine(dirBuild, Path.Combine(Core.Utils.GetExeDir(), "packet-lengths.json")))));




            _server = new GameServer(services);

            // hook the assembly unloading event to signalt the cancellation token
            AppDomain.CurrentDomain.ProcessExit += (a, b) => Dispose();
            Console.CancelKeyPress += (a, b) =>
            {
                Dispose();
                b.Cancel = true;
            };

            try
            {
                AsyncContext.Run(async () =>
                {
                    await _server.Start(_cts.Token).ContinueWith(t =>
                    {
                        if (t.Exception != null)
                        {
                            HandleAggregateException(t.Exception);
                        }
                    }, _cts.Token);
                });
            }
            catch (TaskCanceledException)
            {
                // expected
            }

            Dispose();
        }

        private bool _isDisposed = false;

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                _server.Dispose();
                _cts.Dispose();
            }
        }
    }
}