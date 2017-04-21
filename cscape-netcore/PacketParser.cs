using System;
using System.Diagnostics;

namespace cscape
{
    public static class PacketParser
    {
        public static void Parse(GameServer server, Blob packetStream)
        {
            while (packetStream.CanRead())
            {
                // peek everything untill we're 100% have the packet.
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
                        Undefined(server, opcodePeek);
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
                        Undefined(server, opcode);
                        break;
                    default:
                        lenPayload = (byte) lenType;
                        break;
                }

                Debug.Assert(lenPayload == lenPayloadPeek);

                // todo : build packet, read payload
                packetStream.ReadBlock(trash, 0, lenPayload);

                Console.WriteLine("Packet:\n" +
                                  $"\tOpcode: {opcode}\n" +
                                  $"\t   Len: {lenPayload}\n");
            }
        }

        private static byte[] trash = new byte[1000];

        private static void Undefined(GameServer server, byte opcode)
        {
            var msg = $"Undefined packet opcode: {opcode}";
            server.Log.Warning(typeof(PacketParser), msg);
            Debug.Fail(msg);
            // todo : drop player when we're sent undefined packets

#if DEBUG
            server.Database.Packet.Reload();
#endif
        }

    }
}