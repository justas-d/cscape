using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.InteractingEntity
{
    public interface IInteractingEntity
    {
        int Id { get; }

        [NotNull]
        Entity Entity { get; }
    }
}