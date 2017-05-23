using System;
using System.Diagnostics;
using CScape.Core.Injection;

namespace CScape.Dev.Tests.Impl
{
    public class TestLogger : ILogger
    {
        private void LogDebug(string msg) => Trace.TraceInformation(msg);
        private void LogWarning(string msg) => Trace.TraceWarning(msg);
        private void LogException(string msg) => Trace.TraceError(msg);
        private void Log(string msg) => Trace.WriteLine(msg);

        public void Debug(object s, string msg, string file = "unknown file", int line = -1)
        {
            LogDebug($"[{s}] {msg}");
        }

        public void Normal(object s, string msg, string file = "unknown file", int line = -1)
        {
            Log($"[{s}] {msg}");
        }

        public void Warning(object s, string msg, string file = "unknown file", int line = -1)
        {
            LogWarning($"[{s}] {msg} at {file}({line})");
        }

        public void Exception(object s, string msg, Exception ex, string file = "unknown file", int line = -1)
        {
            LogException($"[{s}] {msg} Exception: {ex} LOGGED at {file}({line})");
        }
    }
}