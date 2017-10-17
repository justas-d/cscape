using System.Collections.Generic;
using CScape.Core.Game.Entities;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    public interface INpcFactory
    {
        [NotNull]
        IEntitySystem EntitySystem { get; }
        [NotNull]
        IReadOnlyList<EntityHandle> All { get; }
        
        // TODO : maybe replace definition id with an INpcDefinition interface?
        [NotNull]
        EntityHandle Create(string name, int definitionId);
        [CanBeNull]
        EntityHandle Get(int id);
    }
}