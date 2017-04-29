using System;

namespace CScape.Game.Entity
{
    public sealed class Transform
    {
        public AbstractEntity Entity { get; }
        private readonly Observatory _observatory;
        private int _regionX;
        private int _regionY;

        public const int MinRegionBorder = 16;
        public const int MaxRegionBorder = 88;

        public ushort X { get; private set; }
        public ushort Y { get; private set; }
        public byte Z { get; private set; }

        public int RegionX
        {
            get => _regionX;
            private set
            {
                _regionX = value;
                BaseX = (ushort) (_regionX * 8);
            }
        }

        public int RegionY
        {
            get => _regionY;
            private set
            {
                _regionY = value;
                BaseY = (ushort) (_regionY * 8);
            }
        }

        public ushort BaseX { get; private set; }
        public ushort BaseY { get; private set; }

        public int LocalX { get; private set; }
        public int LocalY { get; private set; }

        /// <exception cref="ArgumentOutOfRangeException">Z cannot be larger than 4.</exception>
        public Transform(AbstractEntity entity, ushort x, ushort y, byte z)
        {
            Entity = entity;

            switch (entity)
            {
                case IObserver observer:
                    _observatory = observer.Observatory;
                    break;
            }

            SetPosition(x, y, z, false);
        }

        /// <exception cref="ArgumentOutOfRangeException">Z cannot be larger than 4.</exception>
        public void SetPosition(ushort x, ushort y, byte z, bool updateObservatories = true)
        {
            if (z > 4) throw new ArgumentOutOfRangeException($"{nameof(z)} cannot be larger than 4.");

            // don't do anything if we're trying to set the position to where we're at right now.
            if (x == X && y == Y && z == Z)
                return;

            X = x;
            Y = y;
            Z = z;

            RegionX = (X >> 3) - 6;
            RegionY = (Y >> 3) - 6;

            LocalX = x - (8 * RegionX);
            LocalY = y - (8 * RegionY);

            Update(updateObservatories);
            _observatory?.Clear();
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
                RegionX -= 4;
            }
            else if (LocalX >= MaxRegionBorder)
            {
                dx = -4 * 8;
                RegionX += 4;
            }

            if (LocalY < MinRegionBorder)
            {
                dy = 4 * 8;
                RegionY -= 4;
            }
            else if (LocalY >= MaxRegionBorder)
            {
                dy = -4 * 8;
                RegionY += 4;
            }

            if (dx != 0 || dy != 0)
            {
                LocalX += dx;
                LocalY += dy;
            }

            X = (ushort) (BaseX + LocalX);
            Y = (ushort) (BaseY + LocalY);

            // todo : some sort of faster way of find observables that can see this transform
            // iterating over all players is pretty stupid but it works for now
            // IObservers don't necessarily have to be a player as well.

            if (updateObservatories)
                foreach (var p in Entity.Server.Players.Values)
                    p.Observatory.PushObservable(Entity);
        }
    }
}