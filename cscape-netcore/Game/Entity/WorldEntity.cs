using System;
using CScape.Data;
using CScape.Game.World;
using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    public abstract class WorldEntity : IWorldEntity
    {
        public uint UniqueEntityId { get; }
        public Transform Position { get; }

        public PlaneOfExistance PoE { get; private set; }
        public GameServer Server { get; }

        [NotNull] private readonly IdPool _idPool;

        public bool IsDestroyed { get; private set; }

        /// <summary>
        /// Lightweight transform constructor.
        /// Transform position must be initialized.
        /// </summary>
        // ReSharper disable once NotNullMemberIsNotInitialized (SetPoE sets it)
        protected WorldEntity(
            [NotNull] GameServer server,
            [NotNull] IdPool idPool,
            PlaneOfExistance poe = null)
        {
            _idPool = idPool ?? throw new ArgumentNullException(nameof(idPool));
            Server = server ?? throw new ArgumentNullException(nameof(server));

            UniqueEntityId = _idPool.NextId();
            InitPoE(poe, Server.Overworld);
            Position = new Transform(this);
        }

        ~WorldEntity()
        {
            if (!IsDestroyed)
            {
                _idPool.FreeId(UniqueEntityId);
                Server.Log.Debug(this, $"Destroyed unfreed entity id {UniqueEntityId}");
            }
        }

        private void InitPoE(PlaneOfExistance fromCtor, [NotNull] PlaneOfExistance overworld)
        {
            if (overworld == null) throw new ArgumentNullException(nameof(overworld));
            var poe = fromCtor ?? overworld;

            PoE = poe;
            PoE.AddEntity(this);
        }

        /// <summary>
        /// Cleanly switches the PoE of the entity.
        /// </summary>
        public void SwitchPoE([NotNull] PlaneOfExistance newPoe)
        {
            if (newPoe == null) throw new ArgumentNullException(nameof(newPoe));
            if (newPoe == PoE)
                return;

            PoE.RemoveEntity(this);
            PoE = newPoe;
            PoE.AddEntity(this);
        }

        public abstract bool CanSee(IWorldEntity ent);
        
        public void Destroy()
        {
            if (IsDestroyed)
            {
                Server.Log.Warning(this, "Tried to destroy a destroyed entity.");
                return;
            }

            PoE.RemoveEntity(this);
            _idPool.FreeId(UniqueEntityId);
            
            InternalDestroy();

            IsDestroyed = true;
        }

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