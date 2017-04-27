using System;
using System.Collections.Generic;
using CScape.Data;
using CScape.Game.Entity;

namespace CScape.Network.Packet
{
    public sealed class MovementPacketHandler : IPacketHandler
    {
        public int[] Handles { get; } =
        {
            248, // map walk
            98,
            164,
            36
        };

        public const int MaxTiles = 25;

        public void Handle(Player player, int opcode, Blob packet)
        {
            var length = packet.Buffer.Length;

        }
    }
}
