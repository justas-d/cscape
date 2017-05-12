using System;
using CScape.Core.Game.World;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    public abstract class AbstractTransform : ITransform
    {
        private (int x, int y) _local;
        private (int x, int y) _clientRegion;
        public const int MaxZ = 4;
        public const int MinRegionBorder = 16;
        public const int MaxRegionBorder = 88;

        public IWorldEntity Entity { get; }

        public int X { get; protected set; }
        public int Y { get; protected set; }
        public byte Z { get; protected set; }

        public Region Region { get; private set; }
        public PlaneOfExistance PoE { get; private set; }

        public (int x, int y) Base { get; private set; }

        public (int x, int y) ClientRegion => _clientRegion;
        public (int x, int y) Local => _local;

        /// <summary>
        /// Needs initialization (coords, PoE if not set, region)
        /// </summary>
        // ReSharper disable once NotNullMemberIsNotInitialized
        protected AbstractTransform([NotNull] IWorldEntity entity)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        }

        public void SwitchPoE(PlaneOfExistance newPoe)
        {
            if (newPoe == PoE)
                return;

            PoE?.RemoveEntity(this);
            PoE = newPoe;
            PoE.AddEntity(this);

            UpdateRegion();
        }

        public int AbsoluteDistanceTo(ITransform other) => Math.Abs(other.X - X) + Math.Abs(other.Y - Y);
        public int MaxDistanceTo(ITransform other) => Math.Max(Math.Abs(X - other.X), Math.Abs(Y - other.Y));
        public void Teleport(int x, int y) => Teleport(x, y, Z);
        protected abstract void InternalSetPosition(int x, int y, byte z);
        protected abstract void InternalRecalc();

        public void Teleport(int x, int y, byte z)
        {
            if (z > MaxZ) throw new ArgumentOutOfRangeException($"{nameof(z)} cannot be larger than 4.");

            X = x;
            Y = y;
            Z = z;

            _clientRegion = ((X >> 3) - 6, (Y >> 3) - 6);
            _local = (X - (8 * ClientRegion.x), Y - (8 * ClientRegion.y));

            Entity.NeedsSightEvaluation = true;
            InternalSetPosition(x, y, z);

            Recalc();
        }

        public void TransformLocals(int tx, int ty)
        {
            if (tx == 0 && ty == 0) return;

            _local.x += tx;
            _local.y += ty;

            Recalc();
        }

        private void UpdateRegion()
        {
            var region = PoE.GetRegion((X >> Region.Shift, Y >> Region.Shift));

            if (Region == region) return;

            Region?.RemoveEntity(Entity);
            Region = region;
            Region.AddEntity(this);

            Entity.NeedsSightEvaluation = true;
        }

        private void Recalc()
        {
            // update locals and client region
            if (Local.x < MinRegionBorder)
            {
                _local.x += 32;
                _clientRegion.x -= 4;
            }
            else if (Local.x >= MaxRegionBorder)
            {
                _local.x -= 32;
                _clientRegion.x += 4;
            }

            if (Local.y < MinRegionBorder)
            {
                _local.y += 32;
                _clientRegion.y -= 4;
            }
            else if (Local.y >= MaxRegionBorder)
            {
                _local.y -= 32;
                _clientRegion.y += 4;
            }

            Base = (_clientRegion.x * 8, _clientRegion.y * 8);

            // sync locals to globals
            X = Base.x + _local.x;
            Y = Base.y + _local.y;

            Entity.NeedsSightEvaluation = true;
            UpdateRegion();
        }
    }
}