using System;
using System.Net;
using CScape;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace cscape_dev
{
    public class JsonGameServerConfig : IGameServerConfig
    {
        public class JsonEndpointConverter : JsonConverter
        {
            private const char Delimiter = ':';

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var raw = JToken.Load(reader).ToString().Split(Delimiter);
                return new IPEndPoint(IPAddress.Parse(raw[0]), Convert.ToInt32(raw[1]));
            }

            public override bool CanConvert(Type objectType) => objectType == typeof(IPEndPoint);
        }

        public string Version { get; }
        public int Revision { get; }
        public string PrivateLoginKeyDir { get; }
        public string Greeting { get; }
        public int MaxPlayers { get; }
        [JsonConverter(typeof(JsonEndpointConverter))]
        public EndPoint ListenEndPoint { get; }
        public int Backlog { get; }

        [JsonConstructor]
        private JsonGameServerConfig([NotNull] string version, int revision,
            [NotNull] string privateLoginKeyDir, int maxPlayers,
            [NotNull] EndPoint listenEndPoint, int backlog, string greeting)
        {
            if (backlog <= 0) throw new ArgumentOutOfRangeException(nameof(backlog));
            if (maxPlayers <= 0) throw new ArgumentOutOfRangeException(nameof(maxPlayers));
            if (revision <= 0) throw new ArgumentOutOfRangeException(nameof(revision));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Revision = revision;
            PrivateLoginKeyDir = privateLoginKeyDir ?? throw new ArgumentNullException(nameof(privateLoginKeyDir));
            MaxPlayers = maxPlayers;
            ListenEndPoint = listenEndPoint ?? throw new ArgumentNullException(nameof(listenEndPoint));
            Backlog = backlog;
            Greeting = greeting;
        }
    }
}