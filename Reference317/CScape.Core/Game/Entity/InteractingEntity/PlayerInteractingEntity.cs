using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Entity.InteractingEntity;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.InteractingEntity
{
    public class PlayerInteractingEntity : IInteractingEntity
    {
        public short Id { get; }
        
        public IEntityHandle Entity { get; }

        public PlayerInteractingEntity([NotNull] IPlayerComponent player)
        {
            Entity = player.Parent.Handle;
            Id = (short) (player.InstanceId + 32769);
        }
    }
}