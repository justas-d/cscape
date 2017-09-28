using System;
using JetBrains.Annotations;

namespace CScape.Core.Game.NewEntity
{
    public sealed class PlayerFactory
    {
        [NotNull]
        public IEntitySystem EntitySystem { get; }

        private EntityPrefab _prefab;

        public PlayerFactory([NotNull] IEntitySystem entitySystem)
        {
            EntitySystem = entitySystem ?? throw new ArgumentNullException(nameof(entitySystem));
            var builder = new EntityPrefabBuilder();
        }
    }
}
