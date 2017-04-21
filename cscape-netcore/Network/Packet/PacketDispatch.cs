using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace CScape.Network.Packet
{
    public class PacketDispatch
    {
        private readonly Dictionary<int, IPacketHandler> _handlers = new Dictionary<int, IPacketHandler>();
        public GameServer Server { get; }

        public PacketDispatch([NotNull] GameServer server)
        {
            Server = server ?? throw new ArgumentNullException(nameof(server));
            RegisterAssembly(server.GetType().GetTypeInfo().Assembly);
        }

        public void Handle([NotNull] Game.Entity.Player player, int opcode, [NotNull] Blob packet)
        {
            if (packet == null) throw new ArgumentNullException(nameof(packet));

            if (_handlers.ContainsKey(opcode))
            {
                _handlers[opcode].Handle(player, opcode, packet);
                return;
            }

            Server.Log.Debug(this, $"Unhandled packet opcode: {opcode}.");
        }

        public void RegisterAssembly([NotNull] Assembly asm)
        {
            if (asm == null) throw new ArgumentNullException(nameof(asm));

            foreach (var type in asm.GetTypes())
            {
                if (!type.GetInterfaces().Contains(typeof(IPacketHandler))) continue;

                var handler = (IPacketHandler) Activator.CreateInstance(type);
                var opcodes = (int[]) handler.GetType().GetRuntimeProperty("Handles").GetValue(handler);

                foreach (var op in opcodes)
                {
                    if (_handlers.ContainsKey(op))
                    {
                        var existing = _handlers[op];
                        Server.Log.Warning(this,
                            $"Assembly: {asm.GetName().Name} " +
                            $"IPacketHandler: {handler.GetType().Name} " +
                            $"Opcode: {op} overrides " +
                            $"Assembly: {existing.GetType().GetTypeInfo().Assembly.GetName().Name} " +
                            $"IPacketHandler: {existing.GetType().Name}");
                    }

                    _handlers[op] = handler;
                    Server.Log.Debug(this, $"Registered {handler.GetType().Name} IPacketHandler.");
                }
            }
        }
    }
}
