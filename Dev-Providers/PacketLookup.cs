using System.Collections.Generic;
using System.IO;
using CScape.Network.Packet;
using Newtonsoft.Json;

namespace CScape.Dev.Providers
{
    public class PacketLookup : IPacketLengthLookup
    {
        private readonly string _dir;

        [JsonProperty("Incoming")]
        private Dictionary<byte, int> _incoming;
        [JsonProperty("Outgoing")]
        private Dictionary<byte, int> _outgoing;

        [JsonConstructor]
        private PacketLookup(Dictionary<byte, int> incoming, Dictionary<byte, int> outgoing)
        {
            _incoming = incoming;
            _outgoing = outgoing;
        }

        public PacketLookup(string dir)
        {
            _dir = dir;
            Reload();
        }

        public void Reload()
        {
            var cpy = JsonConvert.DeserializeObject<PacketLookup>(File.ReadAllText(_dir));
            _incoming = cpy._incoming;
            _outgoing = cpy._outgoing;
        }

        public PacketLength GetIncoming(byte id) => (PacketLength)_incoming[id];
        public PacketLength GetOutgoing(byte id) => (PacketLength)_outgoing[id];
    }
}