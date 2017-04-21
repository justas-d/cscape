using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace CScape
{
    public class Logger
    {
        public GameServer Server { get; }

        internal Logger([NotNull] GameServer parent)
        {
            Server = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public event EventHandler<LogEventArgs> LogReceived = delegate { };

        [DebuggerStepThrough]
        internal void Debug(object s, string msg, [CallerFilePath] string file = "unknown file",[CallerLineNumber] int line = -1)
            => LogReceived(s, new LogEventArgs(file, line, msg, LogSeverity.Debug));

        [DebuggerStepThrough]
        internal void Normal(object s, string msg, [CallerFilePath] string file = "unknown file", [CallerLineNumber] int line = -1)
            => LogReceived(s, new LogEventArgs(file, line, msg, LogSeverity.Normal));

        [DebuggerStepThrough]
        internal void Warning(object s, string msg, [CallerFilePath] string file = "unknown file", [CallerLineNumber] int line = -1)
            => LogReceived(s, new LogEventArgs(file, line, msg, LogSeverity.Warning));

        [DebuggerStepThrough]
        internal void Exception(object s, string msg, Exception ex, [CallerFilePath] string file = "unknown file", [CallerLineNumber] int line = -1)
            => LogReceived(s, new LogEventArgs(file, line, msg, LogSeverity.Exception, ex));

    }
}