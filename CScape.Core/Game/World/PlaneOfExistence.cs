using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.World
{
    public class PlaneOfExistence : IEnumerable<EntityHandle>
    {
        [NotNull] public string Name { get; }

        [NotNull]  public IGameServer Server { get; }
        public bool IsOverworld => ReferenceEquals(Server.Overworld, this);

        private ILogger Log { get; }

        private readonly HashSet<EntityHandle> _entities = new HashSet<EntityHandle>();

        protected Dictionary<(int, int), Region> Regions { get; } 
            = new Dictionary<(int, int), Region>();

        public PlaneOfExistence([NotNull] IGameServer server, [NotNull] string name)
        {
            Server = server ?? throw new ArgumentNullException(nameof(server));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Log = server.Services.ThrowOrGet<ILogger>();
        }

        protected virtual void InternalRemoveEntity([NotNull]ServerTransform ent) { }
        protected virtual void InternalAddEntity([NotNull] ServerTransform ent) { }

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

        /// <summary>
        /// !!Should only be called by ITransform.
        /// </summary>
        internal void RemoveEntity([NotNull] ServerTransform owningTransform)
        {
            if (owningTransform == null) throw new ArgumentNullException(nameof(owningTransform));

            var ent = owningTransform.Parent;

            if (!ContainsEntity(ent.Handle))
                return;

            _entities.Remove(ent.Handle);
            InternalRemoveEntity(owningTransform);
        }

        /// <summary>
        /// !!Should only be called by ITransform.
        /// </summary>
        internal void RegisterNewEntity([NotNull] ServerTransform transform)
        {
            if (transform == null) throw new ArgumentNullException(nameof(transform));

            // an entity can only belong to one poe at a time.
            if (transform.PoE != this)
            {
                Debug.Fail("PoE tried to AddEntity on a entity that is in a different PoE.");
                throw new InvalidOperationException("PoE tried to AddEntity on a entity that is in a different PoE.");
            }

            var ent = transform.Parent;
                
            if (ContainsEntity(ent.Handle))
                return;

            _entities.Add(ent.Handle);
            
            InternalAddEntity(transform);
        }

        public bool ContainsEntity([NotNull] EntityHandle handle)
        {
            return _entities.Contains(handle);
        }

        public IEnumerator<EntityHandle> GetEnumerator()
            => _entities.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public override string ToString()
            => $"Plane of existence: {Name}";
    }
};