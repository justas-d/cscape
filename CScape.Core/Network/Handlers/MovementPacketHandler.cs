using System.Linq;
using CScape.Core.Data;
using CScape.Core.Game.Entity;

namespace CScape.Core.Network.Handlers
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

            for (var i = 0; i < deltaWaypoints.Length; i++)
                deltaWaypoints[i] = ((sbyte) packet.ReadByte(), (sbyte) packet.ReadByte());

            packet.ReadByte();
            var reference = deltaWaypoints[0];

            if (player.TeleportToDestWhenWalking)
            {
                var expX = player.Transform.Base.x + reference.x;
                var expY = player.Transform.Base.y + reference.y;

                if (deltaWaypoints.Length > 1)
                {
                    expX += deltaWaypoints.LastOrDefault().x;
                    expY += deltaWaypoints.LastOrDefault().y;
                }

                player.ForceTeleport(expX, expY);
                return;
            }

            player.Movement.Directions = new ByReferenceWithDeltaWaypointsDirectionsProvider(
                player.Transform, reference, deltaWaypoints);
        }
    }
}
