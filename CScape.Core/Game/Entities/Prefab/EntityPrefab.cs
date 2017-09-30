using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Prefab
{
    public sealed class EntityPrefab
    {
        public const string DefaultName = "Prefab";

        [CanBeNull]
        public IEnumerable<ComponentPrefab> ComponentPrefabs { get; }

        [NotNull]
        public IEnumerable<Action<Entity>> Setups { get; }
 
        [NotNull]
        public string Name { get; }

        public EntityPrefab(
            [NotNull] IEnumerable<Action<Entity>> setups,
            IEnumerable<ComponentPrefab> componentPrefabs = null, 
            [NotNull] string name = DefaultName)
        {
            Setups = setups ?? throw new ArgumentNullException(nameof(setups));
            ComponentPrefabs = componentPrefabs;
            Name = name ?? DefaultName;
        }
    }
}