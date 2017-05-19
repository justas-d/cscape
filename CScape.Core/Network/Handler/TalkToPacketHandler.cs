using System;
using CScape.Core.Data;
using CScape.Core.Game.Entity;

namespace CScape.Core.Network.Handler
{
    public sealed class TalkToPacketHandler : IPacketHandler
    {
        public int[] Handles { get; } = {155};

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

            // todo : actually do something productive on Talk-To packets
            // todo : WAIT for the player to be one tile next to the entity before doing any talk-to logic

            // for now, test npc stuff
            npc.Say("Hello world!");
            var rng = new Random();
            npc.NpcDefinitionId = (short)rng.Next(0, 100);
        }
    }
}
