using System;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Game.Entity.MovementAction;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;

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

        public void Handle(IEntity entity, PacketMessage packet)
        {
            var npcId = packet.Data.ReadInt16();
            var npc = _npcs.Get(npcId);
            if (npc == null)
            {
                entity.SystemMessage($"Attempted to talk to unregistered npc id {npcId}", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Entity);
                return;
            }

            var action = entity.GetMovementAction();
            if (action == null) return;

            action.CurrentAction = new TalkToNpcAction(entity.Handle, npc);
        }
    }
}
