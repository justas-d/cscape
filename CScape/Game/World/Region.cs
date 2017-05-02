using System;
using System.Collections.Generic;
using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Game.World
{
    public class Region
    {
        [NotNull] public PlaneOfExistance Poe { get; }
        public ushort X { get; }
        public ushort Y { get; }

        public const int Size = 16;
        public const int Shift = 4;

        [NotNull] public HashSet<Player> Players { get; } = new HashSet<Player>();

        public Region([NotNull] PlaneOfExistance poe, ushort x, ushort y)
        {
            Poe = poe ?? throw new ArgumentNullException(nameof(poe));
            X = x;
            Y = y;
        }

        public void AddEntity([NotNull] IWorldEntity ent)
        {
            if (ent == null) throw new ArgumentNullException(nameof(ent));
            if (ent.IsDestroyed)
            {
                ent.Server.Log.Warning(this, $"Tried to add destroyed entity {ent} to region at {X} {Y}");
                return;
            }

            // verify regions
            // entity region must be set to this before AddEntity
            if (ent.Position.Region != this)
                throw new InvalidOperationException("ent.Position.Region must be set to the AddEntity region.");

            switch (ent)
            {
                case Player p:
                    p.DebugMsg($"Region.AddEntity {X} {Y}", ref p.DebugRegion);
                    Players.Add(p);
                    break;
            }
        }

        public void RemoveEntity([NotNull] IWorldEntity ent)
        {
            if (ent == null) throw new ArgumentNullException(nameof(ent));

            switch (ent)
            {
                case Player p:
                    p.DebugMsg($"Region.RemoveEntity {X} {Y}", ref p.DebugRegion);
                    Players.Remove(p);
                    break;
            }
        }

        /// <summary>
        /// Returns the nearby regions that surround this one as well as this region itself.
        /// </summary>
        public IEnumerable<Region> GetNearbyInclusive()
        {
            yield return Poe.GetRegion(X + 1, Y);
            yield return Poe.GetRegion(X + 1, Y + 1);
            yield return Poe.GetRegion(X + 1, Y - 1);

            yield return Poe.GetRegion(X - 1, Y);
            yield return Poe.GetRegion(X - 1, Y + 1);
            yield return Poe.GetRegion(X - 1, Y - 1);

            yield return this;
            yield return Poe.GetRegion(X, Y + 1);
            yield return Poe.GetRegion(X, Y - 1);
        }
    }
}