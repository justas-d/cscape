using System.Collections.Generic;
using System.IO;
using CScape.Core.Network;
using Newtonsoft.Json;

namespace CScape.Core.Json
{
    public class JsonPacketDatabase : IPacketDatabase
    {
        private readonly string _dir;

        [JsonProperty("Incoming")]
        private Dictionary<byte, int> _incoming;
        [JsonProperty("Outgoing")]
        private Dictionary<byte, int> _outgoing;

        [JsonConstructor]
        private JsonPacketDatabase(Dictionary<byte, int> incoming, Dictionary<byte, int> outgoing)
        {
            _incoming = incoming;
            _outgoing = outgoing;
        }

        public JsonPacketDatabase(string dir)
        {
            _dir = dir;
            Reload();
        }

        public void Reload()
        {
            var cpy = JsonConvert.DeserializeObject<JsonPacketDatabase>(File.ReadAllText(_dir));
            _incoming = cpy._incoming;
            _outgoing = cpy._outgoing;
        }

        public PacketLength GetIncoming(byte id) => (PacketLength)_incoming[id];
        public PacketLength GetOutgoing(byte id) => (PacketLength)_outgoing[id];
    }
}