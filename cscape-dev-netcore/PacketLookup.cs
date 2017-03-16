using System.Collections.Generic;
using cscape;
using Newtonsoft.Json;

namespace cscape_dev
{
    public class PacketLookup : IPacketLengthLookup
    {
        [JsonProperty("Incoming")]
        private readonly Dictionary<byte, int> _incoming;
        [JsonProperty("Outgoing")]
        private readonly Dictionary<byte, int> _outgoing;

        [JsonConstructor]
        private PacketLookup(Dictionary<byte, int> incoming, Dictionary<byte, int> outgoing)
        {
            _incoming = incoming;
            _outgoing = outgoing;
        }

        public PacketLength GetIncoming(byte id) => (PacketLength)_incoming[id];
        public PacketLength GetOutgoing(byte id) => (PacketLength)_outgoing[id];
    }
}