using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using CScape.Core.Data;
using CScape.Core.Injection;
using CScape.Core.Network.Sync;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    public abstract class WorldEntity : IWorldEntity
    {
        public uint UniqueEntityId { get; }
        public IGameServer Server { get; }

        public ITransform Transform { get; protected set; }

        public bool NeedsSightEvaluation { get; set; }
        public bool IsDestroyed { get; private set; }

        protected IIdPool IdPool { get; }
        public ILogger Log { get; }

        /// <summary>
        /// Transform must be set initialized.
        /// </summary>
        // ReSharper disable once NotNullMemberIsNotInitialized (SetPoE sets it)
        protected WorldEntity([NotNull] IServiceProvider services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            IdPool = services.ThrowOrGet<IIdPool>();
            Server = services.ThrowOrGet<IGameServer>();
            Log = services.ThrowOrGet<ILogger>();

            UniqueEntityId = IdPool.NextEntity();
        }

        ~WorldEntity()
        {
            if (!IsDestroyed)
            {
                IdPool.FreeEntity(UniqueEntityId);
                Log.Debug(this, $"Destroyed unfreed entity id {UniqueEntityId}");
            }
        }

        public virtual bool CanSee(IWorldEntity ent)
        {
            const int viewRange = 15;
            if (ent.IsDestroyed)
                return false;

            if (ent.Transform.Z != Transform.Z)
                return false;

            if (!Transform.PoE.ContainsEntity(ent))
                return false;

            return ent.Transform.MaxDistanceTo(Transform) <= viewRange;
        }

        public void Destroy()
        {
            if (IsDestroyed)
            {
                Log.Warning(this, "Tried to destroy a destroyed entity.");
                return;
            }

            NeedsSightEvaluation = true;
            Transform.PoE.RemoveEntity(Transform);

            foreach (var cnt in _containers)
            {
                Debug.Assert(cnt != null);
                cnt.Remove(this);
            }

            Debug.Assert(_containers.Count == 0);
            InternalDestroy();

            IdPool.FreeEntity(UniqueEntityId);
            IsDestroyed = true;
        }

        IEnumerable<IRegisteredCollection> IWorldEntity.Containers
            => _containers;

        public void RegisterContainer(IRegisteredCollection cont)
        {
            if (cont == null) throw new ArgumentNullException(nameof(cont));
            _containers = _containers.Add(cont);
        }

        public void UnregisterContainer(IRegisteredCollection cont)
        {
            if (cont == null) throw new ArgumentNullException(nameof(cont));
            _containers = _containers.Remove(cont);
        }

        public virtual bool CanBeSeenBy(IObserver ent) => true;

        private ImmutableList<IRegisteredCollection> _containers 
            = ImmutableList<IRegisteredCollection>.Empty;

        protected virtual void InternalDestroy() { }

        public abstract void Update(IMainLoop loop);

        public override string ToString() => $"Entity (UEI: {UniqueEntityId})";

        #region IEquatable


        public override int GetHashCode()
        {
            return (int) UniqueEntityId;
        }

        #endregion


        /// <summary>
        /// Indicates whether this entity is equal to any other entity based on the equality of their <see cref="UniqueEntityId"/>
        /// </summary>
        public bool Equals(IEntity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return UniqueEntityId == other.UniqueEntityId;
        }
    }
}