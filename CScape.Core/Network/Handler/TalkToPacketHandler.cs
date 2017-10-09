using CScape.Core.Data;
using CScape.Core.Game.Entities.MovementAction;
using CScape.Core.Game.Entity;

namespace CScape.Core.Network.Handler
{
    public sealed class TalkToPacketHandler : IPacketHandler
    {
        public byte[] Handles { get; } = {155};

        public void Handle(Player player, int opcode, Blob packet)
        {
            // read
            var npcId = packet.ReadInt16();

            // get 
            var npc = player.Server.Npcs.GetById(npcId);

            // verify
            if (npc == null)
            {
                player.Log.Warning(this, $"Attempted to talk to unregistered npc id {npcId}");
                return;
            }

            player.Movement.MoveAction = new TalkToNpcAction(player, npc);
        }
    }
}
