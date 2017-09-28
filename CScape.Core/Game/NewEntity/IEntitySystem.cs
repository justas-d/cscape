using System.Collections.Generic;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.NewEntity
{
    public interface IEntitySystem
    {
        IReadOnlyDictionary<EntityHandle, Entity> Entities { get; }

        IGameServer Server { get; }

        EntityHandle Create([NotNull] string name, bool useClientTransform = false);
        void Destroy([NotNull] EntityHandle handle);
        Entity Get([NotNull] EntityHandle entityHandle);
        bool IsDead(EntityHandle handle);
    }
}