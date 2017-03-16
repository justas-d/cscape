using System;
using System.Net;
using cscape;
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
        public int MaxPlayers { get; }
        [JsonConverter(typeof(JsonEndpointConverter))]
        public EndPoint ListenEndPoint { get; }
        public int Backlog { get; }

        [JsonConstructor]
        private JsonGameServerConfig(string version, int revision, string privateLoginKeyDir, int maxPlayers, EndPoint listenEndPoint, int backlog)
        {
            Version = version;
            Revision = revision;
            PrivateLoginKeyDir = privateLoginKeyDir;
            MaxPlayers = maxPlayers;
            ListenEndPoint = listenEndPoint;
            Backlog = backlog;
        }
    }
}