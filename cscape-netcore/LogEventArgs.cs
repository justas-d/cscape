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

        /// <exception cref="ArgumentNullException"><paramref name="file"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="message"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException">Severity is an out-of-range value.</exception>
        internal LogEventArgs([NotNull] string file, int line, [NotNull] string message, LogSeverity severity, [CanBeNull]  Exception exception = null)
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
}