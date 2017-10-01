using System.Diagnostics;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Component
{
    [RequiresFragment(typeof(PlayerComponent))]
    public sealed class PacketDebugComponent : IEntityComponent
    {
        public Entity Parent { get; }
        public int Priority { get; }

        [NotNull]
        private PlayerComponent Player
        {
            get
            {
                Debug.Assert(Parent.Components != null, "Parent.Components != null");
                // ReSharper disable once AssignNullToNotNullAttribute
                return Parent.Components.Get<PlayerComponent>();
            }
        }

        public PacketDebugComponent(Entity parent)
        {
            Parent = parent;
        }

        public void ReceiveMessage(EntityMessage msg)
        {
            switch (msg.Event)
            {
                case EntityMessage.EventType.UnhandledPacket:
                {
                    var packet = msg.AsUnhandledPacket();
                    Player.SendMessage($"Unhandled packet opcode: {packet.Opcode:000}");
                    break;
                }
                case EntityMessage.EventType.UndefinedPacket:
                {
                    var packet = msg.AsUndefinedPacket();
                    Parent.Log.Warning(this, $"Undefined packet opcode: {packet.Opcode}");

#if DEBUG
                    Debug.Fail($"Undefined packet opcode: {packet.Opcode}");
#endif
                    break;
                }
            }
        }

        public void Update(IMainLoop loop)
        {
            
        }
    }
}