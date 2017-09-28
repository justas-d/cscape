using System.Diagnostics;

namespace CScape.Core.Game.NewEntity
{
    public sealed class EntityMessage
    {
        private readonly object _data;

        public IEntityComponent Sender { get; }
        public EventType Event { get; }

        public enum EventType
        {
            TookDamage,
            JustDied,
            Move,
            HealedHealth,
            Logout,
            /* etc */
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
        public (int, int) AsMove() => AssertCast<(int, int)>(EventType.Move);
        public int AsHealedHealth() => AssertCast<int>(EventType.HealedHealth);
        public bool AsLogout() => AssertTrue(EventType.Logout);
    }
}