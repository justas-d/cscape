using System.Reflection;
using CScape.Data;
using CScape.Game.Commands;
using CScape.Game.Entity;

namespace CScape.Network.Packet
{
    public sealed class CommandPacketHandler : IPacketHandler
    {
        public GameServer Server { get; }
        public int[] Handles { get; } = { 103 };

        private readonly CommandDispatch _cmds = new CommandDispatch();

        public CommandPacketHandler(GameServer server)
        {
            Server = server;
            _cmds.RegisterAssembly(Server.GetType().GetTypeInfo().Assembly);
        }

        public void Handle(Player player, int opcode, Blob packet)
        {
            if (packet.TryReadString(255, out string cmd))
            {
                if (!_cmds.Dispatch(player, cmd))
                    player.SendSystemChatMessage($"Unknown command: \"{cmd}\"");
            }
        }
    }
}