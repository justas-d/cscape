using System;
using System.Collections.Generic;
using System.Linq;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.NewEntity
{
    public interface IVisionResolver : IEntityComponent
    {
        bool CanBeSeenBy(Entity ent);
    }

    /// <summary>
    /// Tracks what the entity can see
    /// </summary>
    public sealed class VisionComponent : IEntityComponent
    {
        public const int DefaultViewRange = 15;

        public Entity Parent { get; }

        /// <summary>
        /// "Can see up to n tiles".
        /// </summary>
        public int ViewRange { get; } = DefaultViewRange;

        public VisionComponent(Entity parent)
        {
            
            Parent = parent;
        }

        public void Update(IMainLoop loop)
        {
            
        }

        public bool CanSee(Entity ent)
        {
            var resolver = ent.GetComponent<IVisionResolver>();
            if (resolver != null)
            {
                return resolver.CanBeSeenBy(ent);
            }

            return Parent.GetTransform().ChebyshevDistanceTo(ent.GetTransform()) <= ViewRange;
        }

        public IEnumerable<EntityHandle> GetVisibleEntities()
        {
            return Parent.GetTransform().Region.GetNearbyInclusive()
                .SelectMany(e => e.Entities)
                .Where(handle => !handle.IsDead())
                .Where(handle => CanSee(handle.Get()));
        }

        public void ReceiveMessage(EntityMessage msg)
        {
        }
    }

    public sealed class PlayerComponent : IEntityComponent, IEquatable<PlayerComponent>
    {
        public int PlayerId { get; }

        [NotNull]
        public Entity Parent { get; }

        [NotNull]

        public string Username { get; }

        public PlayerComponent([NotNull] Entity parent, [NotNull] string username
            , int playerId)
        {
            PlayerId = playerId;
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Username = username ?? throw new ArgumentNullException(nameof(username));
        }

        public void Update(IMainLoop loop)
        {

        }

        public void ReceiveMessage(EntityMessage msg)
        {

        }

        public bool Equals(PlayerComponent other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Username, other.Username, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is PlayerComponent && Equals((PlayerComponent) obj);
        }

        public override int GetHashCode()
        {
            return Username.GetHashCode() * 31;
        }
    }
}
