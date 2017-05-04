using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using CScape.Data;
using CScape.Game.World;
using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    public abstract class WorldEntity : IWorldEntity
    {
        public uint UniqueEntityId { get; }
        public ITransform Transform { get; protected set; }

        public GameServer Server { get; }

        [NotNull] private readonly IdPool _idPool;

        public bool NeedsSightEvaluation { get; set; }
        public bool IsDestroyed { get; private set; }

        /// <summary>
        /// Transform must be set initialized.
        /// </summary>
        // ReSharper disable once NotNullMemberIsNotInitialized (SetPoE sets it)
        protected WorldEntity(
            [NotNull] GameServer server,
            [NotNull] IdPool idPool)
        {
            _idPool = idPool ?? throw new ArgumentNullException(nameof(idPool));
            Server = server ?? throw new ArgumentNullException(nameof(server));

            UniqueEntityId = _idPool.NextId();
        }

        ~WorldEntity()
        {
            if (!IsDestroyed)
            {
                _idPool.FreeId(UniqueEntityId);
                Server.Log.Debug(this, $"Destroyed unfreed entity id {UniqueEntityId}");
            }
        }

        public abstract bool CanSee(IWorldEntity ent);

        public void Destroy()
        {
            if (IsDestroyed)
            {
                Server.Log.Warning(this, "Tried to destroy a destroyed entity.");
                return;
            }

            Transform.PoE.RemoveEntity(Transform);

            foreach (var cnt in _containers)
            {
                Debug.Assert(cnt != null);
                cnt.Remove(this);
            }

            Debug.Assert(_containers.Count == 0);
            InternalDestroy();

            _idPool.FreeId(UniqueEntityId);
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

        private ImmutableList<IRegisteredCollection> _containers 
            = ImmutableList<IRegisteredCollection>.Empty;

        protected virtual void InternalDestroy() { }

        public abstract void Update(MainLoop loop);
        public abstract void SyncTo(ObservableSyncMachine sync, Blob blob, bool isNew);

        #region IEquatable

        public bool Equals(IEntity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return UniqueEntityId == other.UniqueEntityId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IEntity) obj);
        }

        public override int GetHashCode()
        {
            return (int) UniqueEntityId;
        }

        #endregion
    }
}