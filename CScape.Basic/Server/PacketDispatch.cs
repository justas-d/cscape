using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CScape.Core;
using CScape.Core.Data;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using CScape.Core.Network.Handlers;
using JetBrains.Annotations;

namespace CScape.Basic.Server
{
    public class PacketDispatch : IPacketDispatch
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _log;
        private readonly Dictionary<int, IPacketHandler> _handlers = new Dictionary<int, IPacketHandler>();

        public PacketDispatch(IServiceProvider services)
        {
            _services = services;
            _log = _services.ThrowOrGet<ILogger>();

            RegisterAssembly(typeof(CScape.Core.Constant).GetTypeInfo().Assembly);
        }

        public void Handle(Player player, int opcode, Blob packet)
        {
            if (packet == null) throw new ArgumentNullException(nameof(packet));

            player.Connection.UpdateLastPacketReceivedTime();

            if (_handlers.ContainsKey(opcode))
            {
                if (player.DebugPackets)
                    player.SendSystemChatMessage($"{opcode:000} {_handlers[opcode].GetType().Name}");

                _handlers[opcode].Handle(player, opcode, packet);
                return;
            }

            if (player.DebugPackets)
                player.SendSystemChatMessage(opcode.ToString());

            _log.Debug(this, $"Unhandled packet opcode: {opcode}.");
        }

        public void RegisterAssembly([NotNull] Assembly asm)
        {
            if (asm == null) throw new ArgumentNullException(nameof(asm));

            foreach (var type in asm.GetTypes())
            {
                if (!type.GetInterfaces().Contains(typeof(IPacketHandler))) continue;

                // create instance of the handler
                // some handler classes might have a IServiceProvider arg in their ctor, handle it
                IPacketHandler handler;

                try
                {
                    handler = (IPacketHandler)Activator.CreateInstance(type, _services);
                }
                catch(Exception)
                {
                    handler = (IPacketHandler)Activator.CreateInstance(type);
                }

                if (handler == null)
                {
                    _log.Warning(this, $"Failed to instantiate IPacketHandler {type.Name}: Could not find valid constructor. Only public constructors that take onle GameServer as a paramater or ones that take no params are considered valid.");
                    continue;
                }

                var opcodes = (int[]) handler.GetType().GetRuntimeProperty("Handles").GetValue(handler);

                foreach (var op in opcodes)
                {
                    if (_handlers.ContainsKey(op))
                    {
                        var existing = _handlers[op];
                        _log.Warning(this,
                            $"Assembly: {asm.GetName().Name} " +
                            $"IPacketHandler: {handler.GetType().Name} " +
                            $"Opcode: {op} overrides " +
                            $"Assembly: {existing.GetType().GetTypeInfo().Assembly.GetName().Name} " +
                            $"IPacketHandler: {existing.GetType().Name}");
                    }

                    _handlers[op] = handler;
                    _log.Debug(this, $"Registered {handler.GetType().Name} IPacketHandler.");
                }
            }
        }
    }
}
