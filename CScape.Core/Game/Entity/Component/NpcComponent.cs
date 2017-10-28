using System;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Component
{
    public class NpcComponent : EntityComponent, INpcComponent
    {
        private readonly Action<NpcComponent> _destroyCallback;
        public override int Priority => (int)ComponentPriority.Npc;

        public short DefinitionId { get; private set; }
        public short NpcId { get; }

        public NpcComponent(
            IEntity parent,
            short defId,
            short npcId,
            [NotNull] Action<NpcComponent> destroyCallback)
            :base(parent)
        {
            _destroyCallback = destroyCallback;
            DefinitionId = defId;
            NpcId = npcId;
        }

        public void ChangeDefinitionId(short newId)
        {
            DefinitionId = newId;
            Parent.SendMessage(new DefinitionChangeMessage(newId));
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case SysMessage.DestroyEntity:
                {
                    _destroyCallback(this);
                    break;
                }
            }
        }

        public override string ToString() 
            => $"Npc {NpcId} def-id {DefinitionId}";
    }
}