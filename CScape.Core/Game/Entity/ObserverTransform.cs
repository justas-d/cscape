using System;
using CScape.Core.Game.World;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    public class ObserverTransform : AbstractTransform
    {
        public static class Factory
        {
            public static ObserverTransform Create(
                IObserver forEntity,
                int x, int y, byte z,
                [CanBeNull] PlaneOfExistance poe = null)
            {
                var transform = new ObserverTransform(forEntity);

                transform.Initialize(x, y, z, poe ?? forEntity.Server.Overworld);

                return transform;
            }
        }

        private readonly IObserver _observer;

        private ObserverTransform(
            [NotNull] IObserver entity) : base(entity)
        {
            _observer = entity;
        }

        private void Initialize(int x, int y, byte z, [NotNull] PlaneOfExistance poe)
        {
            if (poe == null) throw new ArgumentNullException(nameof(poe));

            X = x;
            Y = y;
            Z = z;

            SwitchPoE(poe);
            Teleport(x, y, z);
        }

        protected override void InternalSetPosition(int x, int y, byte z)
        {
            _observer.Observatory.Clear();
        }

        protected override void InternalRecalc()
        {
            _observer.Observatory.ReevaluateSightOverride = true;
        }
    }
}