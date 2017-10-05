using System;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Interface;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    public class NpcComponent : EntityComponent
    {
        private readonly Action<NpcComponent> _destroyCallback;
        public override int Priority { get; }

        public int DefinitionId { get; private set; }
        public int NpcId { get; }

        public NpcComponent(
            Entities.Entity parent,
            int defId,
            int npcId,
            [NotNull] Action<NpcComponent> destroyCallback)
            :base(parent)
        {
            _destroyCallback = destroyCallback;
            DefinitionId = defId;
            NpcId = npcId;
        }

        public void ChangeDefinitionId(int newId)
        {
            DefinitionId = newId;
            Parent.SendMessage(
                new EntityMessage(
                    this, EntityMessage.EventType.DefinitionChange, newId));
        }

        public void Say(string text)
        {
           throw new NotImplementedException();
        }

        public override void ReceiveMessage(EntityMessage msg)
        {
            switch (msg.Event)
            {
                case EntityMessage.EventType.DestroyEntity:
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