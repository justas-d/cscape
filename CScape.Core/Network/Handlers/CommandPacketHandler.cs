using System;
using CScape.Core.Data;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using Microsoft.Extensions.DependencyInjection;

namespace CScape.Core.Network.Handlers
{
    public sealed class CommandPacketHandler : IPacketHandler
    {
        private readonly IServiceProvider _services;
        public int[] Handles { get; } = { 103 };

        public CommandPacketHandler(IServiceProvider services)
        {
            _services = services;
        }

        public void Handle(Player player, int opcode, Blob packet)
        {
            if (packet.TryReadString(255, out string cmd))
            {
                var allFailed = true;

                foreach (var c in _services.GetServices<ICommandHandler>())
                {
                    if (c.Push(player, cmd))
                        allFailed = false;
                }

                if(allFailed)
                    player.SendSystemChatMessage($"Unknown command: \"{cmd}\"");
            }
        }
    }
}