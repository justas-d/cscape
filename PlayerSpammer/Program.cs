using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using CScape.Basic;
using Nito.AsyncEx;

namespace PlayerSpammer
{
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

            var dirBuild = Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.Location);

            // make sure we get notified of unobserved task exceptions
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                if (e.Observed)
                    return;
                HandleAggregateException(e.Exception);
                e.SetObserved();
            };

            var cfg = JsonConvert.DeserializeObject<JsonGameServerConfig>(
                File.ReadAllText(
                    Path.Combine(dirBuild, "config.json")));

            var crypto = CScape.Basic.Server.Utils.GetCrypto(cfg, true);
            var idx = 0;

            // dispatch a thread to run the server

            AsyncContext.Run(async () =>
            {
                while (true)
                {
                    Console.ReadLine();
                    var p = new FakePlayer((short)cfg.Revision, cfg.ListenEndPoint, crypto, $"Fake_{idx++}", "1");
                    Console.WriteLine($"Spawning {p.Username}");

                    await p.Login();
                }
            });
        }
    }
}