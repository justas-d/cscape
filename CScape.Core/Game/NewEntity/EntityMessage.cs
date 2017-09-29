using System;
using System.Diagnostics;
using CScape.Core.Game.World;
using JetBrains.Annotations;

namespace CScape.Core.Game.NewEntity
{
    public sealed class PoeSwitchMessageData
    {
        [CanBeNull]
        public PlaneOfExistence OldPoe { get; }

        [NotNull]
        public PlaneOfExistence NewPoe { get; }

        public PoeSwitchMessageData([CanBeNull] PlaneOfExistence oldPoe, [NotNull] PlaneOfExistence newPoe)
        {
            OldPoe = oldPoe;
            NewPoe = newPoe ?? throw new ArgumentNullException(nameof(newPoe));
        }
    }

    public sealed class TeleportMessageData
    {
        public (int x, int y, int z) OldPos { get; }
        public (int x, int y, int z) NewPos { get; }

        public TeleportMessageData(
            (int x, int y, int z) oldPos,
            (int x, int y, int z) newPos)
        {
            OldPos = oldPos;
            NewPos = newPos;
        }
    }

    public sealed class EntityMessage
    {
        private readonly object _data;

        public IEntityComponent Sender { get; }
        public EventType Event { get; }

        public enum EventType
        {
            TookDamage,
            JustDied,
            Move, /* Moving by delta (ie walking or running) */
            HealedHealth,
            Logout,

            PoeSwitch,
            Teleport, /* Forced movement over an arbitrary size of land */
            
            BeginMovePath, 
            StopMovingAlongMovePath, /* We suddenly stop moving on the current path (direction provider) without actually arriving at the destination */
            ArrivedAtDestination, /* Sent whenever a movement controller's direction provider is done */
        };

        public EntityMessage(IEntityComponent sender, EventType ev, object data)
        {
            _data = data;
            Sender = sender;
            Event = ev;
        }

        private T AssertCast<T>(EventType expected)
        {
            Debug.Assert(expected == Event);
            return (T) _data;
        }

        private bool AssertTrue(EventType expected)
        {
            Debug.Assert(Event == expected);
            return true;
        }
        
        public int AsTookDamage() => AssertCast<int>(EventType.TookDamage);
        public bool AsJustDied() => AssertTrue(EventType.JustDied);
        public MovementMetadata AsMove() => AssertCast<MovementMetadata>(EventType.Move);
        public int AsHealedHealth() => AssertCast<int>(EventType.HealedHealth);
        public bool AsLogout() => AssertTrue(EventType.Logout);
        public PoeSwitchMessageData AsPoeSwitch() => AssertCast<PoeSwitchMessageData>(EventType.PoeSwitch);
        public TeleportMessageData AsTeleport() => AssertCast<TeleportMessageData>(EventType.Teleport);

        public bool AsBeginMovePath() => AssertTrue(EventType.BeginMovePath);
        public bool AsStopMovingAlongMovePath() => AssertTrue(EventType.StopMovingAlongMovePath);
        public bool AsArrivedAtDestination() => AssertTrue(EventType.ArrivedAtDestination);
    }
}