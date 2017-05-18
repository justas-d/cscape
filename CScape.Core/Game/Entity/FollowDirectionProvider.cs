using System;
using CScape.Core.Game.World;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    /// <summary>
    /// Provides directions for walking in a circle pattern.
    /// </summary>
    public sealed class CircleDirectionProvider : IDirectionsProvider
    {
        private int _idx;

        private readonly (sbyte, sbyte)[] _directions =
        {
            DirectionHelper.GetDelta(Direction.West),
            DirectionHelper.GetDelta(Direction.SouthWest),
            DirectionHelper.GetDelta(Direction.South),
            DirectionHelper.GetDelta(Direction.SouthEast),
            DirectionHelper.GetDelta(Direction.East),
            DirectionHelper.GetDelta(Direction.NorthEast),
            DirectionHelper.GetDelta(Direction.North),
            DirectionHelper.GetDelta(Direction.NorthWest),
        };

        public (sbyte x, sbyte y) GetNextDir()
            => _directions[_idx++ % _directions.Length];

        public bool IsDone() => false;
        public void Dispose() { } // ignored
    }


    public sealed class FollowDirectionProvider : IDirectionsProvider
    {
        [NotNull]
        public IMovingEntity Us { get; }

        public IMovingEntity Target { get; }
        private ITransform TargPos => Target.Transform;

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

            if (Math.Abs(target.Item1 - Us.Transform.X) + Math.Abs(target.Item2 - Us.Transform.Y) == 1)
                return DirectionHelper.NoopDelta;

            // todo : collision checking in FollowDirectionProvider

            var diffX = Us.Transform.X < target.Item1 ? (sbyte) 1 : (sbyte) -1;
            var diffY = Us.Transform.Y < target.Item2 ? (sbyte) 1 : (sbyte) -1;

            return (Us.Transform.X != target.Item1 ? diffX : (sbyte) 0,
                Us.Transform.Y != target.Item2 ? diffY : (sbyte) 0);
        }

        public bool IsDone()
            => !Us.CanSee(Target);

        public void Dispose()
        {
            Us.InteractingEntity = null;
        }
    }
}