using CScape.Core.Data;
using CScape.Core.Game.Entity;

namespace CScape.Core.Network.Handlers
{
    public sealed class FollowPacketHandler : IPacketHandler
    {
        public int[] Handles { get; } = { 128, 139 };

        public void Handle(Player player, int opcode, Blob packet)
        {
            var followTarget = player.Server.Players.GetById(packet.ReadInt16());
            if (followTarget == null)
                return;

            player.Movement.Directions = new FollowDirectionProvider(player, followTarget);
            player.InteractingEntity = followTarget;
        }
    }
}