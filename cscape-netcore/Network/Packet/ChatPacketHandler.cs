using CScape.Data;

namespace CScape.Network.Packet
{
    public sealed class ChatPacketHandler : IPacketHandler
    {
        public int[] Handles { get; } = {4};

        public void Handle(Game.Entity.Player player, int opcode, Blob packet)
        {
            var effect = packet.ReadByte();
            var color = packet.ReadByte();

            if (packet.TryReadString(255, out string msg))
                player.Log.Debug(this, $"Effect: {effect} Color: {color}\n\tChat: {msg}");
            else
                player.Log.Warning(this, "Couldn't read chat message.");
        }
    }
}