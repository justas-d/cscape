using System;
using CScape.Core.Game.Entities.Component;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.InteractingEntity
{
    public class PlayerInteractingEntity : IInteractingEntity
    {
        public short Id { get; }
        
        public Entity Entity { get; }

        public PlayerInteractingEntity([NotNull] PlayerComponent player)
        {
            Entity = player?.Parent ?? throw new ArgumentNullException(nameof(player));

            Id = player.PlayerId + 32768;
        }
    }
}