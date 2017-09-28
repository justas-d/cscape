using System;
using System.Collections.Generic;
using System.Linq;
using CScape.Core;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Game.NewEntity
{
    public sealed class EntityPrefabBuilder
    {
        /// <typeparam name="TComponent">The type of the component which will be used to later retrieve the component.</typeparam>
        /// <typeparam name="TInstance">The type of the component which will be instantiated. This type must be derived from <see cref="TComponent"/></typeparam>
        public sealed class PrefabComponentBuilder<TComponent, TInstance>
            where TComponent : IEntityComponent
            where TInstance : TComponent
        {
            private List<Func<object>> CtorParams { get; } = new List<Func<object>>();

            private List<Action<TInstance>> Setups { get; }
                = new List<Action<TInstance>>();

            public PrefabComponentBuilder<TComponent, TInstance> WithCtor(
                params Func<object>[] lambdas)
            {
                CtorParams.AddRange(lambdas);
                return this;
            }

            public PrefabComponentBuilder<TComponent, TInstance> WithSetup(
                Action<TInstance> factory)
            {
                Setups.Add(factory);
                return this;
            }

            public ComponentPrefab Build()
            {
                return new ComponentPrefab(
                    typeof(TComponent), typeof(TInstance), CtorParams,
                    Enumerable.Select(Setups, t => Utils.ActionCast<TInstance, object>(t)));

            }
        }

        private readonly List<PrefabComponentBuilder<IEntityComponent, IEntityComponent>> 
            _components 
                = new List<PrefabComponentBuilder<IEntityComponent, IEntityComponent>>();

        private readonly List<Action<Entity>> _setups = new List<Action<Entity>>();

        private string _name = EntityPrefab.DefaultName;

        public EntityPrefabBuilder(bool useClientTransform = false)
        {
            if (useClientTransform)
            {
                WithComponent<ITransform, ClientTransform>();
            }
            else
            {
                WithComponent<ITransform, ServerTransform>();
            }
        }

        /// <summary>
        /// Adds a component to the prefab.
        /// </summary>
        /// <typeparam name="TComponent">The type of the component which will be used to later retrieve the component.</typeparam>
        /// <typeparam name="TInstance">The type of the component which will be instantiated. This type must be derived from <see cref="TComponent"/></typeparam>
        public PrefabComponentBuilder<TComponent, TInstance> WithComponent<TComponent, TInstance>()
            where TComponent : IEntityComponent
            where TInstance : TComponent 
        {
            var builder = new PrefabComponentBuilder<TComponent, TInstance>();
            _components.Add(builder as PrefabComponentBuilder<IEntityComponent, IEntityComponent>);
            return builder;
        }

        public EntityPrefabBuilder WithSetup(
            Action<Entity> factory)
        {
            _setups.Add(factory);
            return this;
        }

        public EntityPrefabBuilder WithName([NotNull] string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            return this;
        }

        public PrefabComponentBuilder<TInstance, TInstance> WithComponent<TInstance>()
            where TInstance : IEntityComponent
        {
            return WithComponent<TInstance, TInstance>();
        }

        public EntityPrefab Build()
        {
            var comp = Enumerable.Select(_components, builder => builder.Build());
            return new EntityPrefab(_setups, comp, _name);
        }
    }
}