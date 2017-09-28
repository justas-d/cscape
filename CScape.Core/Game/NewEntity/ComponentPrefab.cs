using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace CScape.Core.Game.NewEntity
{
    public sealed class ComponentPrefab
    {
        [NotNull]
        public Type ComponentType { get; }

        [NotNull]
        public Type InstanceType { get; }

        [NotNull]
        public IEnumerable<Func<object>> CtorParams { get; }

        [NotNull]
        public IEnumerable<Action<object>> Setups { get; }

        [CanBeNull] public ConstructorInfo CachedConstructor { get; set; }

        public ComponentPrefab(
            [NotNull] Type componentType,
            [NotNull] Type instanceType,
            [NotNull] IEnumerable<Func<object>> ctorParams,
            [NotNull] IEnumerable<Action<object>> setups)
        {
            ComponentType = componentType ?? throw new ArgumentNullException(nameof(componentType));
            InstanceType = instanceType ?? throw new ArgumentNullException(nameof(instanceType));
            CtorParams = ctorParams ?? throw new ArgumentNullException(nameof(ctorParams));
            Setups = setups ?? throw new ArgumentNullException(nameof(setups));
        }
    }
}