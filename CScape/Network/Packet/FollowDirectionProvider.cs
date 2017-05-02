using System;
using CScape.Game.Entity;
using CScape.Game.World;
using JetBrains.Annotations;

namespace CScape.Network.Packet
{
    public sealed class FollowDirectionProvider : IDirectionsProvider
    {
        [NotNull]
        public IMovingEntity Us { get; }

        public IMovingEntity Target { get; }
        private Transform TargPos => Target.Position;

        public FollowDirectionProvider([NotNull] IMovingEntity us, [NotNull] IMovingEntity target)
        {
            Us = us;
            Target = target ?? throw new ArgumentNullException(nameof(target));
            us.InteractingEntity = target;
        }

        public (sbyte x, sbyte y) GetNextDir()
        {
            // if we're 1 tile away, we are exactly where we want to be.
            // targ might move so we have to persist though.
            var offset = DirectionHelper.Invert(Target.LastMovedDirection);
            var target = (TargPos.X + offset.x, TargPos.Y + offset.y);

            if (Math.Abs(target.Item1 - Us.Position.LocalX) + Math.Abs(target.Item2 - Us.Position.LocalY) == 1)
                return DirectionHelper.NoopDelta;

            // todo : collision checking in FollowDirectionProvider

            var diffX = Us.Position.X < target.Item1 ? (sbyte) 1 : (sbyte) -1;
            var diffY = Us.Position.Y < target.Item2 ? (sbyte) 1 : (sbyte) -1;

            return (Us.Position.X != target.Item1 ? diffX : (sbyte) 0,
                Us.Position.Y != target.Item2 ? diffY : (sbyte) 0);
        }

        public bool IsDone()
            => !Us.CanSee(Target);

        public void Dispose()
        {
            Us.InteractingEntity = null;
        }
    }
}