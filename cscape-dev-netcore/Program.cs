using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using CScape;
using Newtonsoft.Json;

namespace cscape_dev
{
    static class Program
    {
        private static readonly BlockingCollection<LogEventArgs> LogQueue = new BlockingCollection<LogEventArgs>();
        private static GameServer _server;

        private static void HandleAggregateException(AggregateException aggEx)
        {
            foreach (var ex in aggEx.InnerExceptions)
                ExceptionDispatchInfo.Capture(ex).Throw();
            // Enable all CLR exceptions in the exception settings window to see the stack-trace.
        }

        static void Main()
        {
            // make sure we're invariant
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            // config
            var cfg = JsonConvert.DeserializeObject<JsonGameServerConfig>(File.ReadAllText("config.json"));
            _server = new GameServer(cfg, new ServerDatabase("packet-lengths.json"));

            _server.Log.LogReceived += (s, l) => LogQueue.Add(l);

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                if (e.Observed)
                    return;
                HandleAggregateException(e.Exception);
                e.SetObserved();
            };

            ThreadPool.QueueUserWorkItem(o =>
            {
                foreach (var log in LogQueue.GetConsumingEnumerable())
                    WriteLog(log);
            });

            Task.Run(_server.Start).ContinueWith(t =>
            {
                if (t.IsFaulted && t.Exception != null)
                    HandleAggregateException(t.Exception);
            });

            while (true)
                Console.ReadLine();
        }

        private static void WriteIntoDelegate(Action<string> writeDel, LogEventArgs l, double sec)
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

        private static void WriteLog(LogEventArgs log)
        {
            var time = log.Time - _server.StartTime;
#if DEBUG
            if (log.Severity == LogSeverity.Debug)
                WriteIntoDelegate(w => Debug.Write(w), log, time.TotalSeconds);
            else
#endif
                WriteIntoDelegate(Console.Write, log, time.TotalSeconds);
        }
    }
}