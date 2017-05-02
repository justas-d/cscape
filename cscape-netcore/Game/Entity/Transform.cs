using System;
using System.Linq;
using CScape.Game.World;

namespace CScape.Game.Entity
{
    public sealed class Transform
    {
        public const int MaxZ = 4;

        public IWorldEntity Entity { get; }
        private readonly IObserver _asObserver;

        private int _clientRegionX;
        private int _clientRegionY;

        public const int MinRegionBorder = 16;
        public const int MaxRegionBorder = 88;

        public ushort X { get; private set; }
        public ushort Y { get; private set; }
        public byte Z { get; private set; }

        // 8x8 on client
        public int ClientRegionX
        {
            get => _clientRegionX;
            private set
            {
                _clientRegionX = value;
                BaseX = (ushort) (_clientRegionX * 8);
            }
        }

        public int ClientRegionY
        {
            get => _clientRegionY;
            private set
            {
                _clientRegionY = value;
                BaseY = (ushort) (_clientRegionY * 8);
            }
        }

        public Region Region { get; private set; }

        public ushort BaseX { get; private set; }
        public ushort BaseY { get; private set; }

        public int LocalX { get; private set; }
        public int LocalY { get; private set; }

        /// <summary>
        /// Lightweight constructor.
        /// Position must be set immediatelly after.
        /// </summary>
        public Transform(IWorldEntity entity)
        {
            Entity = entity;
            _asObserver = entity as IObserver;

        }

        /// <exception cref="ArgumentOutOfRangeException">Z cannot be larger than MaxZ.</exception>
        public void SetPosition(ushort x, ushort y, byte z, bool updateObservatories = true)
        {
            if (z > MaxZ) throw new ArgumentOutOfRangeException($"{nameof(z)} cannot be larger than 4.");

            X = x;
            Y = y;
            Z = z;

            ClientRegionX = (X >> 3) - 6;
            ClientRegionY = (Y >> 3) - 6;

            LocalX = x - (8 * ClientRegionX);
            LocalY = y - (8 * ClientRegionY);

            _asObserver?.Observatory.Clear();
            Update(updateObservatories);
        }

        public void SetPosition(ushort x, ushort y)
            => SetPosition(x, y, Z);

        /// <summary>
        /// Returns the absolute distance to the given transform.
        /// Does not take into account the z planes of both transforms.
        /// </summary>
        public int AbsoluteDistanceTo(Transform other)
        {
            return Math.Abs(other.X - X) + Math.Abs(other.Y - Y);
        }

        /// <summary>
        /// Returns the maximum distances of absoulte x and y position differences 
        /// between this and the other transform.
        /// </summary>
        public int MaxDistanceTo(Transform other)
        {
            return Math.Max(Math.Abs(X - other.X), Math.Abs(Y - other.Y));
        }

        public void TransformLocals((sbyte, sbyte) tuple)
            => TransformLocals(tuple.Item1, tuple.Item2);

        public void TransformLocals(sbyte tx, sbyte ty)
        {
            // don't do anything if the transform is null
            if (tx == 0 && ty == 0)
                return;

            LocalX += tx;
            LocalY += ty;
            Update();
        }

        private void Update(bool updateObservatories = true)
        {
            var dx = 0;
            var dy = 0;

            if (LocalX < MinRegionBorder)
            {
                dx = 4 * 8;
                ClientRegionX -= 4;
            }
            else if (LocalX >= MaxRegionBorder)
            {
                dx = -4 * 8;
                ClientRegionX += 4;
            }

            if (LocalY < MinRegionBorder)
            {
                dy = 4 * 8;
                ClientRegionY -= 4;
            }
            else if (LocalY >= MaxRegionBorder)
            {
                dy = -4 * 8;
                ClientRegionY += 4;
            }

            if (dx != 0 || dy != 0)
            {
                LocalX += dx;
                LocalY += dy;
            }

            X = (ushort) (BaseX + LocalX);
            Y = (ushort) (BaseY + LocalY);

            // update region
            var region = Entity.PoE.GetRegion(X >> Region.Shift, Y >> Region.Shift);
            if (Region != region)
            {
                Region?.RemoveEntity(Entity);
                Region = region;
                Region.AddEntity(Entity);
            }

            // todo : IObservers don't necessarily have to be a player as well.
            if (updateObservatories)
            {
                foreach (var p in Region.GetNearbyInclusive().SelectMany(r => r.Players))
                    p.Observatory.RecursivePushObservable(Entity);
            }
        }
    }
}