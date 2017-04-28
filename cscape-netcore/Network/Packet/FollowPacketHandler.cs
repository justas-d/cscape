using System;
using CScape.Data;
using CScape.Game.Entity;
using CScape.Game.World;
using JetBrains.Annotations;

namespace CScape.Network.Packet
{
    public sealed class FollowPacketHandler : IPacketHandler
    {
        public int[] Handles { get; } = {128};

        public void Handle(Player player, int opcode, Blob packet)
        {
            var followTarget = player.Server.GetPlayerByPid(packet.ReadInt16());
            if (followTarget == null)
                return;

            player.Movement.Directions = new FollowDirectionProvider(player, followTarget);
        }
    }

    // todo : IDirectionsProvider should be able to return a noop, MovementController should handle it but IDirectionsProvider should still exist and be able to provide further directions.
    public sealed class FollowDirectionProvider : IDirectionsProvider
    {
        [NotNull]
        public IMovingEntity Us { get; }
        public IMovingEntity Target { get; }
        private Transform TargPos => Target.Position;

        private const int MaxRange = 16;

        public FollowDirectionProvider([NotNull] IMovingEntity us, [NotNull] IMovingEntity target)
        {
            Us = us;
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }

        public (sbyte x, sbyte y) GetNextDir()
        {
            // if we're 1 tile away, we are exactly where we want to be.
            // targ might move so we have to persist though.
            if (TargPos.AbsoluteDistanceTo(Us.Position) == 1)
                return DirectionHelper.NoopDelta;

            // todo do a interpolate path to target.
            // todo : collision checking in FollowDirectionProvider

            var offset = DirectionHelper.Invert(Target.LastMovedDirection);
            var target = (TargPos.X + offset.x, TargPos.Y + offset.y);

            var diffX = Us.Position.X < target.Item1 ? (sbyte)1 : (sbyte)-1;
            var diffY = Us.Position.Y < target.Item2 ? (sbyte)1 : (sbyte)-1;

            return (Us.Position.X != target.Item1 ? diffX : (sbyte)0,
                Us.Position.Y != target.Item2 ? diffY : (sbyte)0);
        }

        public bool IsDone()
        {
            if (TargPos.Z != Us.Position.Z)
                return true;

            if (Target.IsDestroyed)
                return true;

            if (TargPos.MaxDistanceTo(Us.Position) > MaxRange)
                return true;

            return false;
        }

        public void Dispose() { }
    }
}