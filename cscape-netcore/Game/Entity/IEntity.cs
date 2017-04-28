using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    public interface IEntity
    {
        uint UniqueEntityId { get; }
        bool IsDestroyed { get; }
        [NotNull] GameServer Server { get; }
    }
}