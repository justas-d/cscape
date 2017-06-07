using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.World
{
    public class PlaneOfExistence : IEnumerable<IWorldEntity>
    {
        [NotNull] public ICollisionProvider Collision { get; }
        [NotNull] public string Name { get; }

        [NotNull]  public IGameServer Server { get; }
        public bool IsOverworld => ReferenceEquals(Server.Overworld, this);

        private ILogger Log { get; }

        private readonly EntityPool<IWorldEntity> _entityPool;
        private bool _isFreed;

        protected Dictionary<(int, int), Region> Regions { get; } = new Dictionary<(int, int), Region>();

        public PlaneOfExistence([NotNull] IGameServer server, ICollisionProvider collision, [NotNull] string name)
        {
            Collision = collision;
            Server = server ?? throw new ArgumentNullException(nameof(server));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Log = server.Services.ThrowOrGet<ILogger>();

            _entityPool = new EntityPool<IWorldEntity>();
            Server.Entities.Add(_entityPool);
        }

        ~PlaneOfExistence()
        {
            if (!_isFreed && !IsOverworld)
            {
                Log.Warning(this, "Finalizer called on unfreed PoE.");
                Free();
            }
        }

        protected virtual void InternalFree() { }
        protected virtual void InternalRemoveEntity([NotNull]IWorldEntity ent) { }
        protected virtual void InternalAddEntity([NotNull] IWorldEntity ent) { }

        /// <summary>
        /// Gets the region coords by global position.
        /// </summary>
        [NotNull]
        public virtual Region GetRegion(int x, int y)
        {
            var key = (x >> Region.Shift, y >> Region.Shift);
            return GetRegion(key);
        }

        [NotNull]
        public virtual Region GetRegion((int rx, int ry) regionCoords)
        {
            if(!Regions.ContainsKey(regionCoords))
                Regions.Add(regionCoords, new Region(this, regionCoords.rx, regionCoords.ry));

            return Regions[regionCoords];
        }

        public void Free()
        {
            if (_isFreed) return;
            if(IsOverworld) return;

            Server.Entities.Remove(_entityPool);
            InternalFree();
            _isFreed = true;
        }

        /// <summary>
        /// !!Should only be called by ITransform.
        /// </summary>
        internal void RemoveEntity([NotNull] ITransform owningTransform)
        {
            if (owningTransform == null) throw new ArgumentNullException(nameof(owningTransform));

            var ent = owningTransform.Entity;

            if (!ContainsEntity(ent))
                return;

            _entityPool.Remove(ent);
            InternalRemoveEntity(ent);
        }

        /// <summary>
        /// !!Should only be called by ITransform.
        /// </summary>
        internal void AddEntity([NotNull] ITransform owningTransform)
        {
            if (owningTransform == null) throw new ArgumentNullException(nameof(owningTransform));

            // an entity can only belong to one poe at a time.
            if (owningTransform.PoE != this)
            {
                Debug.Fail("PoE tried to AddEntity on a entity that is in a different PoE.");
                throw new InvalidOperationException("PoE tried to AddEntity on a entity that is in a different PoE.");
            }

            var ent = owningTransform.Entity;

            if (ContainsEntity(ent))
                return;

            _entityPool.Add(ent);
            InternalAddEntity(ent);
        }

        public bool ContainsEntity([NotNull] IWorldEntity obs)
        {
            if (obs == null) throw new ArgumentNullException(nameof(obs));
            return _entityPool.ContainsId(obs.UniqueEntityId);
        }

        public IEnumerator<IWorldEntity> GetEnumerator()
            => _entityPool.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public override string ToString()
            => $"Plane of existance: {Name}";
    }
};