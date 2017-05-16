using CScape.Core.Data;
using CScape.Core.Game.Entity;

namespace CScape.Core.Network.Handlers
{
    public sealed class ItemOnItemActionPacketHandler : IPacketHandler
    {
        public int[] Handles { get; } = {53};

        public void Handle(Player player, int opcode, Blob packet)
        {
            var d2 = packet.ReadInt16();
            var anInt1283 = packet.ReadInt16();
            var data1 = packet.ReadInt16();
            var anInt1284 = packet.ReadInt16();
            var anInt1285 = packet.ReadInt16();
            var data3 = packet.ReadInt16();

            player.Interfaces.OnActionOccurred();
            player.DebugMsg($"I on I: d2: {d2} anInt1283: {anInt1283} data1: {data1} anInt1284: {anInt1284} anInt1285: {anInt1285} data3: {data3}", ref player.DebugItems);
        }
    }
}
