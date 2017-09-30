using System.Collections.Generic;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Interface
{
    public interface IEntitySystem
    {
        IReadOnlyDictionary<EntityHandle, Entity> All { get; }

        IGameServer Server { get; }

        EntityHandle Create([NotNull] string name);

        void Destroy([NotNull] EntityHandle handle);
        Entity Get([NotNull] EntityHandle entityHandle);
        bool IsDead(EntityHandle handle);
    }
}