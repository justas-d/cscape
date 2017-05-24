using CScape.Core.Game.World;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    public class ObserverClientTransform : ClientTransform
    {
        public static class Factory
        {
            public static ObserverClientTransform Create(
                IObserver forEntity,
                int x, int y, byte z,
                [CanBeNull] PlaneOfExistance poe = null)
            {
                var transform = new ObserverClientTransform(forEntity);

                transform.Initialize(x, y, z, poe ?? forEntity.Server.Overworld);

                return transform;
            }
        }


        private readonly IObserver _observer;

        public ObserverClientTransform(
            [NotNull] IObserver entity) 
            : base((IWorldEntity) entity)
        {
            _observer = entity;
        }

        protected override void InternalSetPosition(int x, int y, byte z)
        {
            base.InternalSetPosition(x, y, z);

            _observer.Observatory.Clear();
            _observer.Observatory.ReevaluateSightOverride = true;
        }

        protected override void InternalMove(sbyte dx, sbyte dy)
        {
            base.InternalMove(dx, dy);

            _observer.Observatory.ReevaluateSightOverride = true;
        }
    }
}