using System;
using System.Collections.Generic;
using System.Diagnostics;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Data;

namespace CScape.Core.Network
{
    public sealed class PacketParser : IPacketParser
    {
        private readonly IPacketDatabase _db;

        public PacketParser(IServiceProvider service)
        {
            _db = service.ThrowOrGet<IPacketDatabase>();
        }

        public IEnumerable<PacketMessage> Parse(CircularBlob stream)
        {
        
            while (stream.CanRead())
            {
                // peek everything untill we 100% have the packet.
                var opcodePeek = stream.Peek();
                var lenType = _db.GetIncoming(opcodePeek);
                var lenPayloadPeek = 0;
                var payloadOffset = 0;

                switch (lenType)
                {
                    case PacketLength.NextByte:
                        if (!stream.CanRead(1)) break;

                        lenPayloadPeek = stream.Peek(1);
                        payloadOffset = 1;
                        break;

                    case PacketLength.NextShort:
                        if (!stream.CanRead(2)) break;

                        lenPayloadPeek = stream.Peek(1) << 8 + stream.Peek(2);
                        payloadOffset = 2;
                        break;

                    case PacketLength.Undefined:
                        yield return PacketMessage.Undefined(opcodePeek);
                        yield break;

                    default:
                        lenPayloadPeek = (byte) lenType;
                        break;
                }

                if (!stream.CanRead(payloadOffset + lenPayloadPeek))
                    break;


                // we can read the whole packet, do so.
                // do some assertions on the way
                var opcode = stream.ReadByte();
                Debug.Assert(opcode == opcodePeek);
                var lenPayload = 0;

                switch (lenType)
                {
                    case PacketLength.NextByte:
                        lenPayload = stream.ReadByte();
                        break;
                    case PacketLength.NextShort:
                        lenPayload = stream.ReadInt16();
                        break;
                    case PacketLength.Undefined:
                        yield return PacketMessage.Undefined(opcodePeek);
                        yield break;
                    default:
                        lenPayload = (byte) lenType;
                        break;
                }

                Debug.Assert(lenPayload == lenPayloadPeek);

                // don't bother creating a new Blob if we're storing nothing.
                if (lenPayload == 0)
                {
                    yield return PacketMessage.Success(opcode, null);
                }
                else
                {
                    var payload = new byte[lenPayload];
                    stream.ReadBlock(payload, 0, lenPayload);

                    yield return PacketMessage.Success(opcode, new Blob(payload));
                }
            }
        }
    }
}
