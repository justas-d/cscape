using System;
using System.Collections.Generic;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Game.World
{
    public class Region : IRegion
    {
        [NotNull] public PlaneOfExistence Poe { get; }

        public int X { get; }
        public int Y { get; }

        public const int Size = 16;
        public const int Shift = 4;

        private readonly HashSet<EntityHandle> _entities = new HashSet<EntityHandle>();

        public IReadOnlyCollection<EntityHandle> Entities => _entities;
        
        private IEnumerable<Region> _nearbyRegions;

        public Region([NotNull] PlaneOfExistence poe, int x, int y)
        {
            Poe = poe ?? throw new ArgumentNullException(nameof(poe));
            X = x;
            Y = y;
        }

        public void GC()
        {
            _entities.RemoveWhere(e => e.IsDead());
        }

        public void AddEntity([NotNull] ServerTransform owningTransform)
        {
            if (owningTransform == null) throw new ArgumentNullException(nameof(owningTransform));

            var ent = owningTransform.Parent;

            // verify regions
            // entity region must be set to this before AddEntity
            if (owningTransform.Region != this)
                throw new InvalidOperationException("ent.Position.Region must be set to the AddEntity region.");

            _entities.Add(ent.Handle);
        }

        public void RemoveEntity([NotNull] ServerTransform ent)
        {
            if (ent == null) throw new ArgumentNullException(nameof(ent));

            _entities.Remove(ent.Parent.Handle);
        }

        /// <summary>
        /// Returns the nearby regions that surround this one as well as this region itself.
        /// </summary>
        public IEnumerable<Region> GetNearbyInclusive()
        {
            return _nearbyRegions ?? (_nearbyRegions = new[]
            {
                Poe.GetRegion((X + 1, Y)),
                Poe.GetRegion((X + 1, Y + 1)),
                Poe.GetRegion((X + 1, Y - 1)),

                Poe.GetRegion((X - 1, Y)),
                Poe.GetRegion((X - 1, Y + 1)),
                Poe.GetRegion((X - 1, Y - 1)),

                this,
                Poe.GetRegion((X, Y + 1)),
                Poe.GetRegion((X, Y - 1))
            });
        }
    }
}