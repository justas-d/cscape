using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Game.World
{
    public class PlaneOfExistance : IEnumerable<IWorldEntity>
    {
        [NotNull] public string Name { get; }

        [NotNull]
        public GameServer Server { get; }
        public bool IsOverworld => Server.Overworld == this;

        private readonly EntityPool<IWorldEntity> _entityPool;
        private bool _isFreed;

        private readonly Dictionary<(ushort, ushort), Region> _regions = 
            new Dictionary<(ushort, ushort), Region>();

        public PlaneOfExistance(string name, [NotNull] GameServer server)
        {
            Name = name;
            Server = server ?? throw new ArgumentNullException(nameof(server));

            _entityPool = new EntityPool<IWorldEntity>();
            Server.Entities.Add(_entityPool);
        }

        ~PlaneOfExistance()
        {
            if (!_isFreed && !IsOverworld)
            {
                Server.Log.Warning(this, "Finalizer called on unfreed PoE.");
                Free();
            }
        }

        protected virtual void InternalFree() { }
        protected virtual void InternalRemoveEntity([NotNull]IWorldEntity ent) { }
        protected virtual void InternalAddEntity([NotNull] IWorldEntity ent) { }

        [NotNull]
        public Region GetRegion(int rx, int ry)
        {
            var key = ((ushort)rx, (ushort)ry);
            if (!_regions.ContainsKey(key))
                _regions.Add(key, new Region(this, (ushort)rx, (ushort)ry));

            return _regions[key];
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
        /// !!Should only be called by AbstractEntity.
        /// </summary>
        internal void RemoveEntity([NotNull] IWorldEntity ent)
        {
            if (ent == null) throw new ArgumentNullException(nameof(ent));

            if(!ContainsEntity(ent))
                return;

            _entityPool.Remove(ent);
            InternalRemoveEntity(ent);
        }

        /// <summary>
        /// !!Should only be called by AbstractEntity.
        /// </summary>
        internal void AddEntity([NotNull] IWorldEntity ent)
        {
            if (ent == null) throw new ArgumentNullException(nameof(ent));

            // an entity can only belong to one poe at a time.
            if (ent.PoE != this)
            {
                Debug.Fail("PoE tried to AddEntity on a entity that is in a different PoE.");
                throw new InvalidOperationException("PoE tried to AddEntity on a entity that is in a different PoE.");
            }

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