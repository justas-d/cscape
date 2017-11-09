using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using CScape.Commands.Extensions;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Entity.Factory;
using CScape.Core.Json;
using CScape.Core.Log;
using CScape.Core.Utility;
using CScape.Models.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;

namespace CScape.Core.Runtime
{
    public class ServerContext : IDisposable
    {
        private GameServer _server;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _isDisposed = false;

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

            services.WithSkillDb();
            services.WithInterfaceDatabase(Path.Combine(MiscUtils.GetExeDir(), "interface-ids.json"));
            services.WithItemDatabase();
            services.WithPacketDatabase(Path.Combine(dirBuild, Path.Combine(MiscUtils.GetExeDir(), "packet-lengths.json")));
            services.WithPacketParser();
            services.WithPacketHandlerCatalogue();
            services.WithPlayerDatabase();
            services.WithCommandHandler(new[] {typeof(CScape.Core.Commands.TestCommandClass).GetTypeInfo().Assembly});

            services.WithPlayerCatalogue(s => new PlayerCatalogue(s));
            services.WithNpcFactory(s => new NpcFactory(s));
            services.WithMainLoop(s => new MainLoop(s));
            services.WithGroundItemFactory(s => new GroundItemFactory(s));
            services.WithEntitySystem(s => new EntitySystem(s));
            services.WithLogger(s => new ConcurrentStdoutLogger(s));
            services.WithConfigurationService(s => new JsonConfigurationService(s, Path.Combine(MiscUtils.GetExeDir(), "config.json")));

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