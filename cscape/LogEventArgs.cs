using System;
using System.ComponentModel;
using JetBrains.Annotations;

namespace cscape
{
    public sealed class LogEventArgs
    {

        public string File { get; }
        public int Line { get; }
        public string Message { get; }
        public LogSeverity Severity { get; }
        public DateTime Time { get; }

        [CanBeNull]
        public Exception Exception { get; }

        internal LogEventArgs([NotNull] string file, int line, [NotNull] string message, LogSeverity severity, [CanBeNull]  Exception exception = null)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (line <= 0) throw new ArgumentOutOfRangeException(nameof(line));
            if (!Enum.IsDefined(typeof(LogSeverity), severity))
                throw new InvalidEnumArgumentException(nameof(severity), (int) severity, typeof(LogSeverity));

            File = file;
            Line = line;
            Message = message;
            Severity = severity;
            Exception = exception;
            Time = DateTime.Now;
        }
    }
}