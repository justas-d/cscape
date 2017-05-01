using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        public const int Shift = 3;

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

    public class PlaneOfExistance : IEnumerable<IWorldEntity>
    {
        [NotNull]
        public GameServer Server { get; }
        public bool IsOverworld => Server.Overworld == this;

        private readonly EntityPool<IWorldEntity> _entityPool;
        private bool _isFreed;

        private readonly Dictionary<(ushort, ushort), Region> _regions = 
            new Dictionary<(ushort, ushort), Region>();

        public PlaneOfExistance([NotNull] GameServer server)
        {
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
                Debug.Fail("PoE tried to AddEntity on a entity that is in a different PoE.");

            if (ContainsEntity(ent))
                return;

            _entityPool.Add(ent);

            /*
            // if we're adding a new observer, push them our observables
            var observer = ent as IObserver;
            if (observer != null)
            {
                observer.Observatory.Clear();
                foreach(var e in this)
                    observer.Observatory.RecursivePushObservable(e);
            }
            */

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
    }
};