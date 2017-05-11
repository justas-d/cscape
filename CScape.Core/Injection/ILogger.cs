using System;
using System.Runtime.CompilerServices;

namespace CScape.Core.Injection
{
    public interface ILogger
    {
        void Debug(object s, string msg, [CallerFilePath] string file = "unknown file",
            [CallerLineNumber] int line = -1);

        void Normal(object s, string msg, [CallerFilePath] string file = "unknown file",
            [CallerLineNumber] int line = -1);

        void Warning(object s, string msg, [CallerFilePath] string file = "unknown file",
            [CallerLineNumber] int line = -1);

        void Exception(object s, string msg, Exception ex, [CallerFilePath] string file = "unknown file",
            [CallerLineNumber] int line = -1);
    }
}