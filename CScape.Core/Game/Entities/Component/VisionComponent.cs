using System.Collections.Generic;
using System.Linq;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Game.Entity;

namespace CScape.Core.Game.Entities.Component
{
    // TODO : view range adjuster component 

    /// <summary>
    /// Tracks what the entity can see
    /// </summary>
    public sealed class VisionComponent : EntityComponent
    {
        public const int DefaultViewRange = 15;

        public override int Priority { get; }

        /// <summary>
        /// "Can see up to n tiles".
        /// </summary>
        public int ViewRange { get; } = DefaultViewRange;

        public VisionComponent(Entity parent)
            :base(parent)
        {
            
        }
        
        public bool CanSee(Entity ent)
        {
            // use resolver if the other entity has one
            var resolver = ent.Components.Get<IVisionResolver>();
            if (resolver != null)
            {
                return resolver.CanBeSeenBy(ent);
            }

            if (!Parent.GetTransform().PoE.ContainsEntity(ent.Handle))
                return false;

            return Parent.GetTransform().ChebyshevDistanceTo(ent.GetTransform()) <= ViewRange;
        }

        public IEnumerable<EntityHandle> GetVisibleEntities()
        {
            return Parent.GetTransform().Region.GetNearbyInclusive().SelectMany(e => e.Entities)
                .Where(handle => !handle.IsDead())
                .Where(handle => CanSee(handle.Get()));
        }

        public override void ReceiveMessage(EntityMessage msg) 
        {
        }
    }
}