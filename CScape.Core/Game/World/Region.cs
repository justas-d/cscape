using System;
using System.Collections.Generic;
using CScape.Core.Game.Entities;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.World;
using JetBrains.Annotations;

namespace CScape.Core.Game.World
{
    public class Region : IRegion
    {
        [NotNull] public IPlaneOfExistence Poe { get; }

        public int X { get; }
        public int Y { get; }

        public const int Size = 16;
        public const int Shift = 4;

        private readonly HashSet<EntityHandle> _entities = new HashSet<EntityHandle>();

        public IReadOnlyCollection<IEntityHandle> Entities => _entities;
        
        private IEnumerable<IRegion> _nearbyRegions;

        public Region([NotNull] IPlaneOfExistence poe, int x, int y)
        {
            Poe = poe ?? throw new ArgumentNullException(nameof(poe));
            X = x;
            Y = y;
        }

        public void GC()
        {
            _entities.RemoveWhere(e => e.IsDead());
        }


        public void AddEntity(ITransform owningTransform)
        {
            if (owningTransform == null) throw new ArgumentNullException(nameof(owningTransform));

            var ent = owningTransform.Parent;

            // verify regions
            // entity region must be set to this before AddEntity
            if (owningTransform.Region != this)
                throw new InvalidOperationException("ent.Position.Region must be set to the AddEntity region.");

            _entities.Add(ent.Handle);
        }

        public void RemoveEntity(ITransform owningTransform)
        {
            if (owningTransform == null) throw new ArgumentNullException(nameof(owningTransform));

            _entities.Remove(owningTransform.Parent.Handle);
        }

        /// <summary>
        /// Returns the nearby regions that surround this one as well as this region itself.
        /// </summary>
        public IEnumerable<IRegion> GetNearbyInclusive()
        {
            return _nearbyRegions ?? (_nearbyRegions = new[]
            {
                Poe.GetRegion(X + 1, Y),
                Poe.GetRegion(X + 1, Y + 1),
                Poe.GetRegion(X + 1, Y - 1),

                Poe.GetRegion(X - 1, Y),
                Poe.GetRegion(X - 1, Y + 1),
                Poe.GetRegion(X - 1, Y - 1),

                this,
                Poe.GetRegion(X, Y + 1),
                Poe.GetRegion(X, Y - 1)
            });
        }
    }
}