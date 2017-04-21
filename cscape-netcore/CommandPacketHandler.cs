using cscape;

namespace cscape
{
    public sealed class CommandPacketHandler : IPacketHandler
    {
        public int[] Handles { get; } = { 103 };

        public void Handle(Player player, int opcode, Blob packet)
        {
            if (packet.TryReadString(255, out string cmd))
                player.Log.Debug(this, $"Command: {cmd}");
            else
                player.Log.Warning(this, "Couldn't commmand.");
        }
    }
}