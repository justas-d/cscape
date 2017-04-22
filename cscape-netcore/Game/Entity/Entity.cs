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
        [NotNull] public PlaneOfExistance PoE { get; private set; }
        [NotNull] public GameServer Server { get; }

        [NotNull] private readonly IdPool _idPool;

        [CanBeNull]
        public virtual MovementController Movement { get; }

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
            SetPoE(poe, Server);
        }

        /// <summary>
        /// More inline constructor.
        /// </summary>
        // ReSharper disable once NotNullMemberIsNotInitialized (SetPoE sets it)
        protected AbstractEntity(
            [NotNull] GameServer server,
            [NotNull] IdPool idPool,
            ushort x, ushort y, byte z,
            PlaneOfExistance poe = null,
            bool needsMovementController = false)
        {
            Server = server ?? throw new ArgumentNullException(nameof(server));
            _idPool = idPool ?? throw new ArgumentNullException(nameof(idPool));
            Position = new Transform(this, x, y, z);
            Movement = needsMovementController ? new MovementController(Position) : null;

            UniqueEntityId = _idPool.NextId();
            SetPoE(poe, Server);
        }

        ~AbstractEntity()
        {
            _idPool.FreeId(UniqueEntityId);
            Server.Log.Debug(this, $"Freeing entity id {UniqueEntityId}");
        }

        public void SetPoE(PlaneOfExistance poe, [NotNull] GameServer server)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));

            PoE = poe ?? server.Overworld;
        }

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