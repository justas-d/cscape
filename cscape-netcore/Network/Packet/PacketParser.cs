using System.Collections.Generic;
using System.Diagnostics;
using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Network.Packet
{
    public static class PacketParser
    {
        public static IEnumerable<(int Opcode, Blob Packet)> Parse([NotNull] Player player, [NotNull] GameServer server, [NotNull] Blob packetStream)
        {
            while (packetStream.CanRead())
            {
                // todo : rewrite the peeking in packet parsing with try/catches and blob state resets on catch
                // peek everything untill we 100% have the packet.
                var opcodePeek = packetStream.Peek();
                var lenType = server.Database.Packet.GetIncoming(opcodePeek);
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
                        Undefined(player, server, opcodePeek);
                        break;

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
                        Undefined(player, server, opcode);
                        break;
                    default:
                        lenPayload = (byte) lenType;
                        break;
                }

                Debug.Assert(lenPayload == lenPayloadPeek);

                var payload = new byte[lenPayload];
                packetStream.ReadBlock(payload, 0, lenPayload);

                yield return (opcode, new Blob(payload));
            }
        }

        private static void Undefined(Player player, GameServer server, byte opcode)
        {
            var msg = $"Undefined packet opcode: {opcode}";
            server.Log.Warning(typeof(PacketParser), msg);
            Debug.Fail(msg);
            player.ForcedLogout();

#if DEBUG
            server.Database.Packet.Reload();
#endif
        }

    }
}