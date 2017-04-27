using System;
using System.Collections.Generic;
using System.Linq;
using CScape.Data;
using CScape.Game.Entity;
using CScape.Game.World;

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
            var len = packet.Buffer.Length - 1;

            (int x, int y) reference = (packet.ReadByte(), packet.ReadByte());

            var deltaWaypoints = new (sbyte x, sbyte y)[(len / 2) - 1];

            Console.WriteLine("Waypoints:");
            for (var i = 0; i < deltaWaypoints.Length; i++)
            {
                deltaWaypoints[i] = ((sbyte) packet.ReadByte(), (sbyte) packet.ReadByte());

                Console.WriteLine($"\t{deltaWaypoints[i].x} {deltaWaypoints[i].y}");
            }

            var isRunning = packet.ReadByte() == 1;

            if (player.TeleporToDestWhenWalking)
            {
                var expX = player.Position.BaseX + reference.x + deltaWaypoints.LastOrDefault().x;
                var expY = player.Position.BaseY + reference.y + deltaWaypoints.LastOrDefault().y;

                player.ForceTeleport((ushort)expX, (ushort)expY);

                return;
            }

            (int x, int y) local = (player.Position.LocalX, player.Position.LocalY);
            var target = reference;
            var wi = 0;

            void IterateInterp()
            {
                foreach (var d in Interpolate(local, target))
                {
                    player.Movement.QueueMovement(d);

                    // keep track of locals ourselves incase transform 
                    //update dx and dy kick in and change locals in the transform
                    local.x += d.x;
                    local.y += d.y;
                }
            }

            IterateInterp(); // move to reference
            while (wi < deltaWaypoints.Length)
            {
                // all our targets are the next delta waypoint + reference
                // ReSharper disable once RedundantAssignment
                target = (reference.x + deltaWaypoints[wi].x, reference.y + deltaWaypoints[wi].y);
                IterateInterp();
                 ++wi;
            } 

            /*
             * TODO: spamming movements fucks up the queue.
             * 
             * This is because we fill the queue with valid movements but they're lost in context BECAUSE they stack.
             * 
             * To fix this, we can completely get rid of the queue and instead replace the queue
             * with an abstract class that allows the MovementController to retrieve
             * movement directions when it needs to do so.
             * 
             * Making this an abstract class will allow for scenarios where we 
             * have all the move data pregenerated and we need to submit that to the controller
             * As
             */
        }

        /*
         *             var deltaWaypoints = new(sbyte x, sbyte y)[(packet.Buffer.Length - 1) / 2];

            if (deltaWaypoints.Length == 0)
                return;

            for (var i = 0; i < deltaWaypoints.Length; i++)
                deltaWaypoints[i] = ((sbyte) packet.ReadByte(), (sbyte) packet.ReadByte());

            (int x, int y) reference = (deltaWaypoints[0].x, deltaWaypoints[0].y);

            deltaWaypoints[0] = (0, 0);

            var isRunning = packet.ReadByte() == 1;

            if (player.TeleporToDestWhenWalking)
            {
                var expX = player.Position.BaseX + reference.x + deltaWaypoints.LastOrDefault().x;
                var expY = player.Position.BaseY + reference.y + deltaWaypoints.LastOrDefault().y;

                player.ForceTeleport((ushort)expX, (ushort)expY);

                return;
            }

            player.Movement.Directions = new ByReferenceWithDeltaWaypointsDirectionsProvider(player.Position
                , reference, deltaWaypoints);
    */
        }
    }
}
