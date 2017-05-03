using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using CScape.Dev.Providers;
using Newtonsoft.Json;

namespace CScape.Dev.Runtime
{
    public class ServerContext
    {
        private readonly BlockingCollection<LogEventArgs> _logQueue = new BlockingCollection<LogEventArgs>();
        public GameServer Server { get; private set; }

        private static void HandleAggregateException(AggregateException aggEx)
        {
            foreach (var ex in aggEx.InnerExceptions)
                ExceptionDispatchInfo.Capture(ex).Throw();
            // Enable all CLR exceptions in the exception settings window to see the stack-trace.
        }

        public void Start()
        {
            // make sure we're invariant
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            // cd into build dir
            var dir = System.IO.Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.Location);
            Directory.SetCurrentDirectory(dir);

            // config
            var cfg = JsonConvert.DeserializeObject<JsonGameServerConfig>(File.ReadAllText("config.json"));
            Server = new GameServer(cfg, new ServerDatabase("packet-lengths.json"));

            Server.Log.LogReceived += (s, l) => _logQueue.Add(l);

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                if (e.Observed)
                    return;
                HandleAggregateException(e.Exception);
                e.SetObserved();
            };

            ThreadPool.QueueUserWorkItem(o =>
            {
                foreach (var log in _logQueue.GetConsumingEnumerable())
                    WriteLog(log);
            });

            Server.Start().GetAwaiter().GetResult();
        }

        private void WriteIntoDelegate(Action<string> writeDel, LogEventArgs l, double sec)
        {
            string header = $"[{sec,4:N6}] ";
            writeDel(header + l.Message);
            if (l.Exception != null)
            {
                writeDel(Environment.NewLine);
                writeDel(new string(' ', header.Length + 1) + $"-> Exception: {l.Exception}");
            }
            writeDel(Environment.NewLine);

        }

        private void WriteLog(LogEventArgs log)
        {
            var time = log.Time - Server.StartTime;
#if DEBUG
            if (log.Severity == LogSeverity.Debug)
                WriteIntoDelegate(w => Debug.Write((string) w), log, time.TotalSeconds);
            else
#endif
                WriteIntoDelegate(Console.Write, log, time.TotalSeconds);
        }

    }
}