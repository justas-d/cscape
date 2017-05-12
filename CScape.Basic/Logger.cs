using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Basic
{
    public class Logger : ILogger
    {
        private readonly IGameServer _server;

        private class LogMessage
        {
            public string File { get; }
            public int Line { get; }
            public string Message { get; }
            public LogSeverity Severity { get; }
            public DateTime Time { get; }

            [CanBeNull]
            public Exception Exception { get; }

            [DebuggerStepThrough]
            internal LogMessage([NotNull] string file, int line, [NotNull] string message, LogSeverity severity, [CanBeNull]  Exception exception = null)
            {
                if (file == null) throw new ArgumentNullException(nameof(file));
                if (message == null) throw new ArgumentNullException(nameof(message));
                if (!Enum.IsDefined(typeof(LogSeverity), severity)) throw new ArgumentOutOfRangeException(nameof(severity));

                File = file;
                Line = line;
                Message = message;
                Severity = severity;
                Exception = exception;
                Time = DateTime.Now;
            }

        }

        private Task _logTask;
        private CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly BlockingCollection<LogMessage> _logQueue = new BlockingCollection<LogMessage>();

        public Logger(IGameServer server)
        {
            _server = server;
            StartLogFlush();
        }

        public void StartLogFlush()
        {
            if(_logTask != null)
                throw new InvalidOperationException("Already flushing logs.");

            _logTask = Task.Run(() =>
            {
                foreach (var log in _logQueue.GetConsumingEnumerable())
                    WriteLog(log);
                
            }, _cancel.Token);
        }

        public void StopLogThread()
        {
            if (_logTask == null)
                throw new InvalidOperationException("Not flushing logs.");

            _cancel.Cancel();
            _cancel = new CancellationTokenSource();
            _logTask = null;
        }

        private void WriteIntoDelegate(Action<string> writeDel, LogMessage l, double sec)
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

        private void WriteLog(LogMessage log)
        {
            var time = log.Time - _server.StartTime;
#if DEBUG
            if (log.Severity == LogSeverity.Debug)
                WriteIntoDelegate(w => System.Diagnostics.Debug.Write(w), log, time.TotalSeconds);
            else
#endif
                WriteIntoDelegate(Console.Write, log, time.TotalSeconds);
        }

        [DebuggerStepThrough]
        public void Debug(object s, string msg, [CallerFilePath] string file = "unknown file", [CallerLineNumber] int line = -1)
            => _logQueue.Add(new LogMessage(file, line, msg, LogSeverity.Debug, null));
            
        [DebuggerStepThrough]
        public void Normal(object s, string msg, [CallerFilePath] string file = "unknown file", [CallerLineNumber] int line = -1)
            => _logQueue.Add(new LogMessage(file, line, msg, LogSeverity.Normal, null));
        [DebuggerStepThrough]
        public void Warning(object s, string msg, [CallerFilePath] string file = "unknown file", [CallerLineNumber] int line = -1)
            => _logQueue.Add(new LogMessage(file, line, msg, LogSeverity.Warning, null));

        [DebuggerStepThrough]
        public void Exception(object s, string msg, Exception ex, [CallerFilePath] string file = "unknown file", [CallerLineNumber] int line = -1)
            => _logQueue.Add(new LogMessage(file, line, msg, LogSeverity.Exception, ex));

    }
}