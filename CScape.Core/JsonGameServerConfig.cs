using System;
using System.Net;
using CScape.Models;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CScape.Core
{
    public class JsonGameServerConfig : IGameServerConfig
    {
        public class JsonEndpointConverter : JsonConverter
        {
            private const char Delimiter = ':';

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var raw = JToken.Load(reader).ToString().Split(Delimiter);
                return new IPEndPoint(IPAddress.Parse(raw[0]), Convert.ToInt32(raw[1]));
            }

            public override bool CanConvert(Type objectType) => objectType == typeof(IPEndPoint);
        }

        public string Version { get; }
        public int MaxNpcs { get; }
        public int Revision { get; }
        public int SocketReceiveTimeout { get; }
        public int TickRate { get; }
        public int EntityGcInternalMs { get; }

        public string PrivateLoginKeyDir { get; }
        public string Greeting { get; }
        public int MaxPlayers { get; }
        [JsonConverter(typeof(JsonEndpointConverter))]
        public EndPoint ListenEndPoint { get; }
        public int Backlog { get; }
        public int SocketSendTimeout { get; }

        [JsonConstructor]
        private JsonGameServerConfig([NotNull] string version, int revision,
            [NotNull] string privateLoginKeyDir, int maxPlayers,
            [NotNull] EndPoint listenEndPoint, int backlog, string greeting, int tickRate, 
            int socketReceiveTimeout, int socketSendTimeout, int entityGcInternalMs, int maxNpcs)
        {
            if (backlog <= 0) throw new ArgumentOutOfRangeException(nameof(backlog));
            if (maxPlayers <= 0) throw new ArgumentOutOfRangeException(nameof(maxPlayers));
            if (revision <= 0) throw new ArgumentOutOfRangeException(nameof(revision));
            if (tickRate <= 0) throw new ArgumentOutOfRangeException(nameof(tickRate));
            if (socketSendTimeout < 0) throw new ArgumentOutOfRangeException(nameof(socketSendTimeout));
            if (socketReceiveTimeout < 0) throw new ArgumentOutOfRangeException(nameof(socketReceiveTimeout));
            if (entityGcInternalMs <= 0) throw new ArgumentOutOfRangeException(nameof(entityGcInternalMs));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Revision = revision;
            PrivateLoginKeyDir = privateLoginKeyDir ?? throw new ArgumentNullException(nameof(privateLoginKeyDir));
            MaxPlayers = maxPlayers;
            ListenEndPoint = listenEndPoint ?? throw new ArgumentNullException(nameof(listenEndPoint));
            Backlog = backlog;
            Greeting = greeting;
            TickRate = tickRate;
            SocketReceiveTimeout = socketReceiveTimeout;
            SocketSendTimeout = socketSendTimeout;
            EntityGcInternalMs = entityGcInternalMs;
            MaxNpcs = maxNpcs;
        }
    }
}