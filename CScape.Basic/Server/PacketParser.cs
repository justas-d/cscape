using System;
using System.Collections.Generic;
using System.Diagnostics;
using CScape.Core;
using CScape.Core.Data;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using CScape.Core.Network;

namespace CScape.Basic.Server
{
    public sealed class PacketParser : IPacketParser
    {
        private readonly IPacketDatabase _db;
        private readonly ILogger _log;
        private readonly IPacketDispatch _dispatch;

        public PacketParser(IServiceProvider service)
        {
            _db = service.ThrowOrGet<IPacketDatabase>();
            _log = service.ThrowOrGet<ILogger>();
            _dispatch = service.ThrowOrGet<IPacketDispatch>();
        }

        public IEnumerable<(int Opcode, Blob Packet)> Parse(Player player)
        {
            void Undefined(byte op)
            {
                var msg = $"Undefined packet opcode: {op}";
                _log.Warning(typeof(PacketParser), msg);
                player.ForcedLogout();

#if DEBUG
                Debug.Fail(msg);
#endif
            }

            var packetStream = player.Connection.InStream;

            while (packetStream.CanRead())
            {
                // peek everything untill we 100% have the packet.
                var opcodePeek = packetStream.Peek();
                var lenType = _db.GetIncoming(opcodePeek);
                var lenPayloadPeek = 0;
                var payloadOffset = 0;

                switch (lenType)
                {
                    case PacketLength.NextByte:
                        if (!packetStream.CanRead(1)) break;

                        lenPayloadPeek = packetStream.Peek(1);
                        payloadOffset = 1;
                        break;

                    case PacketLength.NextShort:
                        if (!packetStream.CanRead(2)) break;

                        lenPayloadPeek = packetStream.Peek(1) << 8 + packetStream.Peek(2);
                        payloadOffset = 2;
                        break;

                    case PacketLength.Undefined:
                        Undefined(opcodePeek);
                        yield break;

                    default:
                        lenPayloadPeek = (byte) lenType;
                        break;
                }

                if (!packetStream.CanRead(payloadOffset + lenPayloadPeek))
                    break;


                // we can read the whole packet, do so.
                // do some assertions on the way
                var opcode = packetStream.ReadByte();
                Debug.Assert(opcode == opcodePeek);
                var lenPayload = 0;

                switch (lenType)
                {
                    case PacketLength.NextByte:
                        lenPayload = packetStream.ReadByte();
                        break;
                    case PacketLength.NextShort:
                        lenPayload = packetStream.ReadInt16();
                        break;
                    case PacketLength.Undefined:
                        Undefined(opcode);
                        yield break;
                    default:
                        lenPayload = (byte) lenType;
                        break;
                }

                Debug.Assert(lenPayload == lenPayloadPeek);

                var payload = new byte[lenPayload];
                packetStream.ReadBlock(payload, 0, lenPayload);

                //  dont bother creating a new Blob() if we're not going to be dispatched to a handler.
                if (_dispatch.CanHandle(opcode))
                    yield return (opcode, new Blob(payload));
                else if (player.DebugPackets)
                {
                    player.SendSystemChatMessage($"Unhandled packet opcode: {opcode:000}");
                }
            }
        }
    }
}
