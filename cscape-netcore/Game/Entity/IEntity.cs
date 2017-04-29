using System;
using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    public interface IEntity : IEquatable<IEntity>
    {
        uint UniqueEntityId { get; }
        bool IsDestroyed { get; }
        [NotNull] GameServer Server { get; }
        [NotNull] Transform Position { get; }
    }
}