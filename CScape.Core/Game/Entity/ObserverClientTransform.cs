using CScape.Core.Game.World;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    // TODO : replace this class with inline calls to _client
    // TODO : actually wait, use messages instead?
    public class ObserverClientTransform : ClientTransform
    {
        private readonly IObserver _observer;

        public ObserverClientTransform(
            [NotNull] IObserver entity) 
            : base(entity)
        {
            _observer = entity;
        }

        protected override void InternalSwitchPoE(PlaneOfExistence newPoe)
        {
            base.InternalSwitchPoE(newPoe);
            _observer.Observatory.Clear();
        }

        protected override void InternalSetPosition(int x, int y, byte z)
        {
            base.InternalSetPosition(x, y, z);
            _observer.Observatory.Clear();
        }

        protected override void InternalMove(sbyte dx, sbyte dy)
        {
            base.InternalMove(dx, dy);

            _observer.Observatory.ReevaluateSightOverride = true;
        }
    }
}