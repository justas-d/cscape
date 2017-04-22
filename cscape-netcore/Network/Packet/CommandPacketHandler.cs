using System;

namespace CScape.Network.Packet
{
    public sealed class CommandPacketHandler : IPacketHandler
    {
        public int[] Handles { get; } = { 103 };

        public void Handle(Game.Entity.Player player, int opcode, Blob packet)
        {
            if (packet.TryReadString(255, out string cmd))
            {
                switch (cmd)
                {
                    case "logout":
                        player.Logout(out _);
                        break;
                    case "forcelogout":
                        player.ForcedLogout();
                        break;
                    case "id":
                        player.SendSystemChatMessage($"UniqueEntityId: {player.UniqueEntityId}");
                        break;
                    case "gc":
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        break;
                    default:
                        player.Log.Debug(this, $"Command: {cmd}");
                        break;

                }
            }
                
            else
                player.Log.Warning(this, "Couldn't commmand.");
        }
    }
}