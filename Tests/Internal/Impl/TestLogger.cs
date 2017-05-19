using System;
using CScape.Core.Injection;

namespace CScape.Dev.Tests.Internal.Impl
{
    public class TestLogger : ILogger
    {
        private void Log(string msg) => System.Diagnostics.Debug.Write(msg);

        public void Debug(object s, string msg, string file = "unknown file", int line = -1)
        {
            Log($"[{s}] {msg}");
        }

        public void Normal(object s, string msg, string file = "unknown file", int line = -1)
        {
            Log($"[{s}] {msg}");
        }

        public void Warning(object s, string msg, string file = "unknown file", int line = -1)
        {
            Log($"[{s}] {msg} at {file}({line})");
        }

        public void Exception(object s, string msg, Exception ex, string file = "unknown file", int line = -1)
        {
            Log($"[{s}] {msg} Exception: {ex} LOGGED at {file}({line})");
        }
    }
}