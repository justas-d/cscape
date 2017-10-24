using System;
using CScape.Core.Game.Entities;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.InteractingEntity;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.InteractingEntity
{
    public class NpcInteractingEntity : IInteractingEntity
    {
        public short Id { get; }
        public IEntityHandle Entity { get; }

        public NpcInteractingEntity([NotNull] INpcComponent npc)
        {
            Entity = npc.Parent.Handle ?? throw new ArgumentNullException(nameof(npc));
            Id = npc.NpcId;
        }
    }
}