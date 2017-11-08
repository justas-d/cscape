using System;
using System.Globalization;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using CScape.Core;
using CScape.Core.Extensions;
using CScape.Core.Json;
using CScape.Models;
using CScape.Models.Data;
using CScape.Models.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;

namespace CSCape.Core.PlayerSpammer
{
    class BasicLogger : ILogger
    {
        public void Debug(object s, string msg, string file = "unknown file", int line = -1)
        {
            Console.WriteLine($"[{s.GetType().Name}] {msg} at {file}({line})");
        }

        public void Normal(object s, string msg, string file = "unknown file", int line = -1)
        {
            Console.WriteLine($"[{s.GetType().Name}] {msg}");
        }

        public void Warning(object s, string msg, string file = "unknown file", int line = -1)
        {
            Console.WriteLine($"[{s.GetType().Name}] {msg} at {file}({line})");
        }

        public void Exception(object s, string msg, Exception ex, string file = "unknown file", int line = -1)
        {
            Console.WriteLine($"[{s.GetType().Name}] {msg} at {file}({line})");
            Console.WriteLine($"Exception: {ex}");
        }
    }


    class Program
    {
        private static void HandleAggregateException(AggregateException aggEx)
        {
            foreach (var ex in aggEx.InnerExceptions)
                ExceptionDispatchInfo.Capture(ex).Throw();
            // Enable all CLR exceptions in the exception settings window to see the stack-trace.
        }

        static void Main() => new Program().Run();

        void Run()
        {
            // make sure we're invariant
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            // make sure we get notified of unobserved task exceptions
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                if (e.Observed)
                    return;
                HandleAggregateException(e.Exception);
                e.SetObserved();
            };

            var builder = new ServiceCollection();
            builder.AddSingleton<ILogger>(new BasicLogger());
            builder.AddSingleton<IConfigurationService>(s => new JsonConfigurationService(s, "config.json"));
            var services = builder.BuildServiceProvider();

            var cfg = services.ThrowOrGet<IConfigurationService>();

            var crypto = CScape.Core.Network.Utils.GetCrypto(cfg.Get(ConfigKey.PrivateLoginKeyDir), true);
            var idx = 0;

            // dispatch a thread to run the server

            AsyncContext.Run(async () =>
            {
                while (true)
                {
                    Console.ReadLine();
                    var p = new FakePlayer((short)cfg.GetInt(ConfigKey.Revision), cfg.GetIpAddress(ConfigKey.ListenEndPoint), crypto, $"Fake_{idx++}", "1");
                    Console.WriteLine($"Spawning {p.Username}");

                    await p.Login();
                }
            });
        }
    }
}