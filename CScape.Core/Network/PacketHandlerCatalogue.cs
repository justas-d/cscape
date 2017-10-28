using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CScape.Core.Network.Handler;
using CScape.Models;
using JetBrains.Annotations;

namespace CScape.Core.Network
{
    public class PacketHandlerCatalogue : IPacketHandlerCatalogue
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _log;
        private readonly Dictionary<int, IPacketHandler> _handlers = new Dictionary<int, IPacketHandler>();

        public PacketHandlerCatalogue(IServiceProvider services)
        {
            _services = services;
            _log = _services.ThrowOrGet<ILogger>();

            RegisterAssembly(GetType().GetTypeInfo().Assembly);
        }

        public IEnumerable<IPacketHandler> All => _handlers.Values;

        public IPacketHandler GetHandler(byte opcode)
        {
            if (_handlers.ContainsKey(opcode))
            {
                return _handlers[opcode];
            }

            return null;
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

                var opcodes = (byte[]) handler.GetType().GetRuntimeProperty("Handles").GetValue(handler);

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
