using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Threading;
using cscape;

namespace cscape_dev
{
    class Program
    {
        private static readonly BlockingCollection<LogEventArgs> LogQueue = new BlockingCollection<LogEventArgs>();

        private static GameServer _server;

        static void Main(string[] args)
        {
            _server = new GameServer(new IPEndPoint(IPAddress.Loopback, 43594));

            _server.Log.LogReceived += (s, l) => LogQueue.Add(l);

            ThreadPool.QueueUserWorkItem(o =>
            {
                foreach (var log in LogQueue.GetConsumingEnumerable())
                    WriteLog(log);
            });


            _server.Start();

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