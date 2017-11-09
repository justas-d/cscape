using System.Collections.Generic;
using System.Linq;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;

namespace CScape.Core.Game.Entity
{
    public static class EntityVision
    {
        public const int DefaultViewRange = 15;

        public static bool IsEntityWithinViewrangeOfOther(IEntity entity, IEntity other, int viewrange)
            => entity.GetTransform().ChebyshevDistanceTo(other.GetTransform()) <= viewrange;
        
        public static bool CanSee(IEntity parent, IEntity other, int viewrange)
        {
            var us = parent.GetTransform();
            var oth = other.GetTransform();

            if (!us.PoE.ContainsEntity(other.Handle))
                return false;

            if (us.Z != oth.Z)
                return false;

            var inRange = IsEntityWithinViewrangeOfOther(parent, other, viewrange);

            // use resolver if the other entity has one
            var resolver = other.Components.Get<IVisionResolver>();
            if (resolver != null)
            {
                return resolver.CanBeSeenBy(parent, inRange);
            }

            return inRange;
        }

        public static IEnumerable<IEntityHandle> GetVisibleEntities(IEntity parent, int viewrange)
        {
            return parent.GetTransform().Region.GetNearbyInclusive().SelectMany(e => e.Entities)
                .Where(handle => !handle.IsDead())
                .Where(handle => !handle.Equals(parent.Handle))
                .Where(handle => CanSee(parent, handle.Get(), viewrange));
        }
    }
}