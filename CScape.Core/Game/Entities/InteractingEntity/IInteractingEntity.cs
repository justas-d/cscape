using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.InteractingEntity
{
    public interface IInteractingEntity
    {
        short Id { get; }

        [CanBeNull]
        EntityHandle Entity { get; }
    }
}