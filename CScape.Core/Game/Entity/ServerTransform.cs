using System;
using CScape.Core.Game.World;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    public class ServerTransform : ITransform
    {
        public const int MaxZ = 4;

        public int X { get; protected set; } = 0;
        public int Y { get; protected set; } = 0;
        public int Z { get; protected set; } = 0;

        public Region Region { get; private set; }
        public PlaneOfExistence PoE { get; private set; }

        public NewEntity.Entity Parent { get; }

        public bool NeedsSightEvaluation { get; set; } = true;

        public ServerTransform([NotNull] NewEntity.Entity parent)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            SwitchPoE(parent.Server.Overworld);
        }

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

        public void Teleport(int x, int y, int z)
        {
            if (z > MaxZ) throw new ArgumentOutOfRangeException($"{nameof(z)} cannot be larger than {MaxZ}.");

            X = x;
            Y = y;
            Z = z;

            NeedsSightEvaluation = true;
            InternalSetPosition(x, y, z);
        }

        public void Move(sbyte dx, sbyte dy)
        {
            // validate 
            if (dx == 0 && dy == 0) return;
            bool IsInvalid(sbyte c) => -1 > c || c > 1;
            if (IsInvalid(dx)) return;
            if (IsInvalid(dy)) return;

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

            Region?.RemoveEntity(Parent);
            Region = region;
            Region.AddEntity(this);

            NeedsSightEvaluation = true;
        }

        protected virtual void InternalSwitchPoE(PlaneOfExistence newPoe) { }
        protected virtual void InternalSetPosition(int x, int y, int z) { }
        protected virtual void InternalMove(sbyte dx, sbyte dy) { }

        public void Update(IMainLoop loop);
    }
}