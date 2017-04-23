using System;
using CScape.Game.Worldspace;
using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    /// <summary>
    /// Something that can be observed and it's observation can be synced.
    /// </summary>
    public abstract class AbstractEntity : IEquatable<AbstractEntity>
    {
        public uint UniqueEntityId { get; }
        [NotNull] public Transform Position { get; }

        // internally it can be null, but for outside viewers it should always exist
        [NotNull] public PlaneOfExistance PoE { get; private set; }
        [NotNull] public GameServer Server { get; }

        [NotNull] private readonly IdPool _idPool;

        [CanBeNull]
        public virtual MovementController Movement { get; }

        public bool IsDestroyed { get; private set; }

        /// <summary>
        /// Facade constructor
        /// </summary>
        // ReSharper disable once NotNullMemberIsNotInitialized (SetPoE sets it)
        protected AbstractEntity(
            [NotNull] GameServer server,
            [NotNull] IdPool idPool,
            [NotNull] Transform pos,
            PlaneOfExistance poe = null,
            MovementController movement = null)
        {
            Position = pos ?? throw new ArgumentNullException(nameof(pos));
            _idPool = idPool ?? throw new ArgumentNullException(nameof(idPool));
            Server = server ?? throw new ArgumentNullException(nameof(server));
            Movement = movement;

            UniqueEntityId = _idPool.NextId();
            InitPoE(poe, Server.Overworld);
        }

        /// <summary>
        /// Player AbstractEntity constructor
        /// </summary>
        //Player will call PoE due to PoE.AddEntity attempting to access Player.Observatory before it is set.
        // ReSharper disable once NotNullMemberIsNotInitialized
        protected AbstractEntity(
            [NotNull] GameServer server,
            [NotNull] IdPool idPool,
            ushort x, ushort y, byte z)
        {
            _idPool = idPool ?? throw new ArgumentNullException(nameof(idPool));
            Server = server ?? throw new ArgumentNullException(nameof(server));
            Position = new Transform(this, x, y, z);
            Movement = new MovementController(Position);
            UniqueEntityId = _idPool.NextId();
        }

        ~AbstractEntity()
        {
            if (!IsDestroyed)
            {
                _idPool.FreeId(UniqueEntityId);
                Server.Log.Debug(this, $"Destroyed unfreed entity id {UniqueEntityId}");
            }
        }

        protected void InitPoE(PlaneOfExistance fromCtor, [NotNull] PlaneOfExistance overworld)
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

        /// <summary>
        /// Destroys the Entity, making sure it doesn't exist in the world any longer, frees up its id.
        /// Overriding this method should be done by overriding the virtual method 
        /// InternalDestroy(), which is called after AbstractEntity.Destroy().
        /// </summary>
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
        /// <summary>
        /// Called every update tick, if scheduled for updating.
        /// The entity is responible for scheduling it's own updates.
        /// No need to call base.Update(). 
        /// </summary>
        /// <param name="loop">The tick loop where the entity can schedule the next update.</param>
        public virtual void Update(MainLoop loop) { }
        public abstract void SyncObservable(ObservableSyncMachine sync, Blob blob, bool isNew);

        #region IEquatable
        public bool Equals(AbstractEntity other)
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
            return Equals((AbstractEntity) obj);
        }

        public override int GetHashCode()
        {
            return (int) UniqueEntityId;
        }
        #endregion
    }
}