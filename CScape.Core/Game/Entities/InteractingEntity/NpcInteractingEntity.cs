using System;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.InteractingEntity
{
    public class NpcInteractingEntity : IInteractingEntity
    {
        public short Id { get; }
        public Entity Entity { get; }

        public NpcInteractingEntity([NotNull] NpcComponent npc)
        {
            Entity = npc.Parent ?? throw new ArgumentNullException(nameof(npc));
            Id = npc.NpcId;
        }
    }
}