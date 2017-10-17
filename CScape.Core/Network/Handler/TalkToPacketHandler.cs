using System;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Entities.MovementAction;
using CScape.Core.Injection;

namespace CScape.Core.Network.Handler
{
    public sealed class TalkToPacketHandler : IPacketHandler
    {
        public byte[] Handles { get; } = {155};

        private INpcFactory _npcs;

        public TalkToPacketHandler(IServiceProvider services)
        {
            _npcs = services.ThrowOrGet<INpcFactory>();
        }

        public void Handle(Game.Entities.Entity entity, PacketMetadata packet)
        {
            var npcId = packet.Data.ReadInt16();
            var npc = _npcs.Get(npcId);
            if (npc == null)
            {
                entity.SystemMessage($"Attempted to talk to unregistered npc id {npcId}");
                return;
            }

            var action = entity.Components.Get<MovementActionComponent>();
            if (action == null) return;

            action.CurrentAction = new TalkToNpcAction(entity.Handle, npc);
        }
    }
}
