using System;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities
{
    public class NpcComponent : EntityComponent, INpcComponent
    {
        private readonly Action<NpcComponent> _destroyCallback;
        public override int Priority { get; }

        public short DefinitionId { get; private set; }
        public int NpcId { get; }

        public NpcComponent(
            Entities.Entity parent,
            short defId,
            int npcId,
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
            Parent.SendMessage(
                new GameMessage(
                    this, GameMessage.Type.DefinitionChange, newId));
        }

        public void Say(string text)
        {
           throw new NotImplementedException();
        }

        public override void ReceiveMessage(GameMessage msg)
        {
            switch (msg.Event)
            {
                case GameMessage.Type.DestroyEntity:
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