using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using cscape;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace cscape_dev
{
    public class JsonGameServerConfig : IGameServerConfig
    {
        public string Version { get; }
        public int Revision { get; }
        public string PrivateLoginKeyDir { get; }
        public int MaxPlayers { get; }
        [JsonConverter(typeof(JsonEndpointConverter))]
        public EndPoint ListenEndPoint { get; }
        public int Backlog { get; }

        public JsonGameServerConfig(string version, int revision, string privateLoginKeyDir, int maxPlayers, EndPoint listenEndPoint, int backlog)
        {
            Version = version;
            Revision = revision;
            PrivateLoginKeyDir = privateLoginKeyDir;
            MaxPlayers = maxPlayers;
            ListenEndPoint = listenEndPoint;
            Backlog = backlog;
        }
    }

    public class JsonEndpointConverter : JsonConverter
    {
        private const char Delimiter = ':';

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var val = (IPEndPoint) value;
            JToken.FromObject($"{val.Address}{Delimiter}{val.Port}").WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var raw = JToken.Load(reader).ToString().Split(Delimiter);
            return new IPEndPoint(IPAddress.Parse(raw[0]), Convert.ToInt32(raw[1]));
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(IPEndPoint);
    }

    static class Program
    {
        private static readonly BlockingCollection<LogEventArgs> LogQueue = new BlockingCollection<LogEventArgs>();
        private static GameServer _server;

        static void Main()
        {
            // config
            var cfg = JsonConvert.DeserializeObject<JsonGameServerConfig>(File.ReadAllText("config.json"));
            _server = new GameServer(cfg);

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