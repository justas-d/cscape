using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace cscape
{
    public class Logger
    {
        public GameServer Server { get; }

        internal Logger([NotNull] GameServer parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            Server = parent;
        }

        public event EventHandler<LogEventArgs> LogReceived = delegate { };

        internal void Debug(object s, string msg, [CallerFilePath] string file = "unknown file",[CallerLineNumber] int line = -1)
            => LogReceived(s, new LogEventArgs(file, line, msg, LogSeverity.Debug));

        internal void Normal(object s, string msg, [CallerFilePath] string file = "unknown file", [CallerLineNumber] int line = -1)
            => LogReceived(s, new LogEventArgs(file, line, msg, LogSeverity.Normal));

        internal void Warning(object s, string msg, [CallerFilePath] string file = "unknown file", [CallerLineNumber] int line = -1)
            => LogReceived(s, new LogEventArgs(file, line, msg, LogSeverity.Warning));

        internal void Exception(object s, string msg, Exception ex, [CallerFilePath] string file = "unknown file", [CallerLineNumber] int line = -1)
            => LogReceived(s, new LogEventArgs(file, line, msg, LogSeverity.Exception, ex));

    }
}