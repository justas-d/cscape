using System;
using System.Linq;
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
            var deltaWaypoints = new (sbyte x, sbyte y)[(packet.Buffer.Length - 1) / 2];

            Console.WriteLine("Waypoints:");
            for (var i = 0; i < deltaWaypoints.Length; i++)
            {
                deltaWaypoints[i] = ((sbyte) packet.ReadByte(), (sbyte) packet.ReadByte());
                Console.WriteLine($"\t{deltaWaypoints[i].x} {deltaWaypoints[i].y}");
            }

            var isRunning = packet.ReadByte() == 1;
            var reference = deltaWaypoints[0];

            if (player.TeleportToDestWhenWalking)
            {
                var expX = player.Position.BaseX + reference.x;
                var expY = player.Position.BaseY + reference.y;

                if (deltaWaypoints.Length > 1)
                {
                    expX += deltaWaypoints.LastOrDefault().x;
                    expY += deltaWaypoints.LastOrDefault().y;
                }

                player.ForceTeleport((ushort) expX, (ushort) expY);
                return;
            }

            player.Movement.IsRunning = isRunning;
            player.Movement.Directions = new ByReferenceWithDeltaWaypointsDirectionsProvider(
                player.Position, reference, deltaWaypoints);
        }
    }
}
