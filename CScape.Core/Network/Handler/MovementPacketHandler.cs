using System.Linq;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Game.Entity.Directions;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Data;
using CScape.Models.Extensions;
using CScape.Models.Game;
using CScape.Models.Game.Entity;

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

        public int MaxTiles { get; } = 25;

        public void Handle(IEntity entity, PacketMessage packet)
        {
            var movement = entity.Components.Get<TileMovementComponent>();
            if (movement == null)
                return;

            var clientPos = entity.GetClientPosition();
            if (clientPos == null)
                return;

            // non-paired
            var rawWaypointNum = packet.Data.Buffer.Length - 1;

            // make sure it's an even number;
            if (rawWaypointNum % 2 != 0)
                return;

            var numWaypoints = rawWaypointNum / 2;

            // check if waypoint count is out of range
            if (0 >= numWaypoints || numWaypoints > MaxTiles)
                return;

            var deltaWaypoints = new(sbyte x, sbyte y)[numWaypoints];

            // read waypoints
            for (var i = 0; i < deltaWaypoints.Length; i++)
                deltaWaypoints[i] = ((sbyte)packet.Data.ReadByte(), (sbyte)packet.Data.ReadByte());

            // read unknown
            packet.Data.ReadByte();

            /* Translate local waypoints into global waypoints */
            var waypoints = new IPosition[deltaWaypoints.Length];
            var reference = new ImmIntVec3(
                deltaWaypoints[0].x + clientPos.Base.X,
                deltaWaypoints[0].y + clientPos.Base.Y, 
                clientPos.Base.Z);

            waypoints[0] = reference;

            for (var i = 1; i < deltaWaypoints.Length; i++)
            {
                waypoints[i] = new ImmIntVec3(
                    reference.X + deltaWaypoints[i].x,
                    reference.Y + deltaWaypoints[i].y,
                    reference.Z);
            }

            // create direction provider for these waypoints
            movement.Directions = new InterpolatedWaypointDirectionsProvider(waypoints);
        }
    }
}