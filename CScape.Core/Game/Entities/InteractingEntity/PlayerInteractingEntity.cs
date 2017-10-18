using CScape.Core.Game.Entities.Component;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.InteractingEntity
{
    public class PlayerInteractingEntity : IInteractingEntity
    {
        public short Id { get; }
        
        public EntityHandle Entity { get; }

        public PlayerInteractingEntity([NotNull] PlayerComponent player)
        {
            Entity = player.Parent.Handle;
            Id = (short) (player.PlayerId + 32768);
        }
    }
}