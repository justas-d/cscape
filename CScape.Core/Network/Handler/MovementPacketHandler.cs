using System;
using System.Linq;
using CScape.Core.Game.Entity;
using CScape.Models.Data;

namespace CScape.Core.Network.Handler
{
    public sealed class MovementPacketHandler : IPacketHandler
    {
        public byte[] Handles { get; } =
        {
            248, // map walk
            98,
            164,
            36
        };

        public int MaxTiles { get;  } =25;

        public void Handle(Player player, int opcode, Blob packet)
        {
            // TODO : rewrite movement packet handling. preferably when we have collision data.

            // non-paired
            var rawWaypointNum = packet.Buffer.Length - 1;

            // make sure it's an even number;
            if (rawWaypointNum % 2 != 0)
                return;

            var numWaypoints = rawWaypointNum / 2;

            // check if waypoint count is out of range
            if (0 >= numWaypoints || numWaypoints > MaxTiles)
                return;

            player.Interfaces.OnActionOccurred();

            var deltaWaypoints = new (sbyte x, sbyte y)[numWaypoints];

            // read waypoints
            for (var i = 0; i < deltaWaypoints.Length; i++)
                deltaWaypoints[i] = ((sbyte) packet.ReadByte(), (sbyte) packet.ReadByte());

            // read unknown
            packet.ReadByte();

            var reference = deltaWaypoints[0];

            // handle tp on walk
            if (player.TeleportToDestWhenWalking)
            {
                var expX = player.ClientTransform.Base.x + reference.x;
                var expY = player.ClientTransform.Base.y + reference.y;

                if (deltaWaypoints.Length > 1)
                {
                    expX += deltaWaypoints.LastOrDefault().x;
                    expY += deltaWaypoints.LastOrDefault().y;
                }

                player.ForceTeleport(expX, expY);
                return;
            }

            // create direction provider for these waypoints
            player.Movement.Directions = new ByReferenceWithDeltaWaypointsDirectionsProvider(
                player.ClientTransform.Local, reference, deltaWaypoints);
        }
    }
}