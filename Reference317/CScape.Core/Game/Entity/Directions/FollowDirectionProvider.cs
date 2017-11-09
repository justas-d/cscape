using System.Linq;
using CScape.Models.Extensions;
using CScape.Models.Game;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Entity.Directions;
using CScape.Models.Game.World;

namespace CScape.Core.Game.Entity.Directions
{
    public sealed class FollowDirectionProvider : IDirectionsProvider
    {
        public IEntityHandle Target { get; }

        public FollowDirectionProvider(IEntityHandle target)
        {
            Target = target;
        }

        private IPosition GetTargetPosition(ITransform transform)
        {
            /* We need to invert the last moved direction because the entities position + inverted last direction = the tile that faces the back of the target entity.
             * That's what we want, so we do that
             */

            if (transform.FacingState.TryConvertToDelta(out var delta))
                return delta.Invert() + transform;

            var inverted = transform.LastMovedDirection.Invert();
            return inverted + transform;
        }

        public GeneratedDirections GetNextDirections(IEntity ent)
        {
            if(Target.IsDead())
                return GeneratedDirections.Noop;

            var entityTranfrom = ent.GetTransform();
            var targetTransform = Target.Get().GetTransform();

            var targetPosition = GetTargetPosition(targetTransform);

            // Use WalkTo pathing, then take two directions from it and conver it to an array.
            // Doing all of these skips us from dealing with enumerators.
            // Since WalkTo is guaranteed to return noops if we're on top of the target, we don't need to worry about going out of range.
            var data = PathingUtils.WalkTo(entityTranfrom, targetPosition).Take(2).ToArray();
            return new GeneratedDirections(data[0], data[1]);
        }

        public bool IsDone(IEntity entity)
        {
            // don't walk toward a dead entity
            if (Target.IsDead())
                return true;

            // use the vision component to resolve vision
            var vision = entity.GetVision();
            if (vision == null)
                return true; // no vision component? bail.

            // we're done for this entity if this entity cannot see the target entity.
            return !vision.CanSee(Target.Get());
        }
    }
}