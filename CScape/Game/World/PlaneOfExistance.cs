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

        private readonly Dictionary<int, Dictionary<int, Region>> _Xregions
            = new Dictionary<int, Dictionary<int, Region>>();

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
            if(!_Xregions.ContainsKey(rx))
                _Xregions.Add(rx, new Dictionary<int, Region>());

            var yReg = _Xregions[rx];

            if(!yReg.ContainsKey(ry))
                yReg.Add(ry, new Region(this, rx, ry));

            return yReg[ry];
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