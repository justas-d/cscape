using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using CScape.Models;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.World;
using JetBrains.Annotations;

namespace CScape.Core.Game.World
{
    public sealed class PlaneOfExistence : IPlaneOfExistence
    {
        [NotNull] public string Name { get; }

        [NotNull]  public IGameServer Server { get; }
        public bool IsOverworld => ReferenceEquals(Server.Overworld, this);

        private ILogger Log { get; }

        private readonly HashSet<IEntityHandle> _entities = new HashSet<IEntityHandle>();

        private Dictionary<(int, int), Region> Regions { get; } 
            = new Dictionary<(int, int), Region>();

        public PlaneOfExistence([NotNull] IGameServer server, [NotNull] string name)
        {
            Server = server ?? throw new ArgumentNullException(nameof(server));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Log = server.Services.ThrowOrGet<ILogger>();
        }

        /// <summary>
        /// Gets the region coords by global position.
        /// </summary>
        [NotNull]
        public IRegion GetRegion(int x, int y)
        {
            var key = (x >> Region.Shift, y >> Region.Shift);
            return GetRegion(key);
        }

        public void GC()
        {
            _entities.RemoveWhere(e => e.IsDead());
            foreach(var region in Regions.Values)
                region.GC();
        }

        [NotNull]
        public IRegion GetRegion((int rx, int ry) regionCoords)
        {
            if(!Regions.ContainsKey(regionCoords))
                Regions.Add(regionCoords, new Region(this, regionCoords.rx, regionCoords.ry));

            return Regions[regionCoords];
        }

        /// <summary>
        /// !!Should only be called by ITransform.
        /// </summary>
        public void RemoveEntity(ITransform owningTransform)
        {
            if (owningTransform == null) throw new ArgumentNullException(nameof(owningTransform));

            var ent = owningTransform.Parent;

            if (!ContainsEntity(ent.Handle))
                return;

            _entities.Remove(ent.Handle);
        }

        /// <summary>
        /// !!Should only be called by ITransform.
        /// </summary>
        public void RegisterNewEntity(ITransform transform)
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
           
        }

        public bool ContainsEntity([NotNull] IEntityHandle handle)
        {
            return _entities.Contains(handle);
        }

        public IEnumerator<IEntityHandle> GetEnumerator()
            => _entities.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public bool Equals(IPlaneOfExistence other) => ReferenceEquals(this, other);

        public override string ToString()
            => $"Plane of existence: {Name}";
    }
}