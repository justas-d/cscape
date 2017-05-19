using CScape.Core.Data;
using CScape.Core.Game.Entity;

namespace CScape.Core.Network.Handler
{
    public sealed class FollowPacketHandler : IPacketHandler
    {
        public int[] Handles { get; } = { 128, 139 };

        public void Handle(Player player, int opcode, Blob packet)
        {
            var id = packet.ReadInt16();
            var target = player.Server.Players.GetById(id);

            if (target == null)
                return;

            if (target.Equals(player))
                return;

            player.Movement.Directions = new FollowDirectionProvider(player, target);
            player.InteractingEntity = target;
        }
    }
}