using System;
using CScape.Core.Game.World;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    public class ServerTransform : ITransform
    {
        public const int MaxZ = 4;

        public IWorldEntity Entity { get; }

        public int X { get; protected set; }
        public int Y { get; protected set; }
        public byte Z { get; protected set; }

        public Region Region { get; private set; }
        public PlaneOfExistence PoE { get; private set; }

        // ReSharper disable once NotNullMemberIsNotInitialized
        public ServerTransform([NotNull] IWorldEntity entity,
            IPosition pos,
            PlaneOfExistence poe = null) : this(entity, pos.X, pos.Y, pos.Z, poe) { }

        // ReSharper disable once NotNullMemberIsNotInitialized
        public ServerTransform([NotNull] IWorldEntity entity,
            int x, int y, byte z,
            PlaneOfExistence poe = null)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));

            Teleport(x, y, z);
            SwitchPoE(poe ?? entity.Server.Overworld);
        }

        /// <summary>
        /// PoE and Region are uninitialized
        /// </summary>
        protected ServerTransform([NotNull] IWorldEntity entity)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        }

        public int AbsoluteDistanceTo(ITransform other) 
            => Math.Abs(other.X - X) + Math.Abs(other.Y - Y);

        public int MaxDistanceTo(ITransform other) 
            => Math.Max(Math.Abs(X - other.X), Math.Abs(Y - other.Y));

        public void Teleport(IPosition pos)
            => Teleport(pos.X, pos.Y, pos.Z);

        public void Teleport(int x, int y) 
            => Teleport(x, y, Z);

        public void SwitchPoE(PlaneOfExistence newPoe)
        {
            if (newPoe == PoE)
                return;

            PoE?.RemoveEntity(this);
            PoE = newPoe;
            PoE.AddEntity(this);

            InternalSwitchPoE(newPoe);
            UpdateRegion();
        }

        public void Teleport(int x, int y, byte z)
        {
            if (z > MaxZ) throw new ArgumentOutOfRangeException($"{nameof(z)} cannot be larger than 4.");

            X = x;
            Y = y;
            Z = z;

            Entity.NeedsSightEvaluation = true;
            InternalSetPosition(x, y, z);
        }

        public void Move(sbyte dx, sbyte dy)
        {
            // validate 
            if (dx == 0 && dy == 0) return;
            bool IsInvalid(ref sbyte c) => -1 > c || c > 1;
            if (IsInvalid(ref dx)) return;
            if (IsInvalid(ref dy)) return;

            // exec
            X += dx;
            Y += dy;

            // todo : Move collision checking
            // todo : handle multi-tile entities in Move()

            InternalMove(dx ,dy);
        }

        protected void UpdateRegion()
        {
            var region = PoE.GetRegion(X, Y);

            if (Region == region) return;

            Region?.RemoveEntity(Entity);
            Region = region;
            Region.AddEntity(this);

            Entity.NeedsSightEvaluation = true;
        }

        protected virtual void InternalSwitchPoE(PlaneOfExistence newPoe) { }
        protected virtual void InternalSetPosition(int x, int y, byte z) { }
        protected virtual void InternalMove(sbyte dx, sbyte dy) { }
    }
}