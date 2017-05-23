using System;
using System.Collections.Generic;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Game.World
{
    public class Region
    {
        [NotNull] public PlaneOfExistance Poe { get; }

        public int X { get; }
        public int Y { get; }

        public const int Size = 16;
        public const int Shift = 4;

        [NotNull]public RegisteredHashSet<Player> Players { get; } = new RegisteredHashSet<Player>();
        [NotNull]public RegisteredHashSet<IObserver> Observers { get; } = new RegisteredHashSet<IObserver>();
        [NotNull]public RegisteredHashSet<IWorldEntity> WorldEntities { get; } = new RegisteredHashSet<IWorldEntity>();
        [NotNull]public RegisteredHashSet<GroundItem> Items { get; } = new RegisteredHashSet<GroundItem>();

        private IEnumerable<Region> _nearbyRegions;

        public Region([NotNull] PlaneOfExistance poe, int x, int y)
        {
            Poe = poe ?? throw new ArgumentNullException(nameof(poe));
            X = x;
            Y = y;
        }

        public void AddEntity([NotNull] ITransform owningTransform)
        {
            if (owningTransform == null) throw new ArgumentNullException(nameof(owningTransform));

            var ent = owningTransform.Entity;
            if (ent.IsDestroyed)
            {
                ent.Log.Warning(this, $"Tried to add destroyed entity {ent} to region at {X} {Y}");
                return;
            }


            // verify regions
            // entity region must be set to this before AddEntity
            if (owningTransform.Region != this)
                throw new InvalidOperationException("ent.Position.Region must be set to the AddEntity region.");

            WorldEntities.Add(ent);

            if (ent is Player p)
            {
                p.DebugMsg($"Region.AddEntity {X} {Y}", ref p.DebugRegion);
                Players.Add(p);
            }
            if (ent is IObserver o)
                Observers.Add(o);
            if (ent is GroundItem item)
                Items.Add(ent);
        }

        public void RemoveEntity([NotNull] IWorldEntity ent)
        {
            if (ent == null) throw new ArgumentNullException(nameof(ent));

            WorldEntities.Remove(ent);

            if (ent is Player p)
            {
                p.DebugMsg($"Region.RemoveEntity {X} {Y}", ref p.DebugRegion);
                Players.Remove(p);
            }
            if (ent is IObserver o)
                Observers.Remove(o);
            if (ent is GroundItem item)
                Items.Remove(item);
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