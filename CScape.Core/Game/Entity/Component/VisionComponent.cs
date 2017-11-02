using System.Collections.Generic;
using System.Linq;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Entity.InteractingEntity;
using CScape.Models.Game.Message;

namespace CScape.Core.Game.Entity.Component
{
    // TODO : view range adjuster component 


    /// <summary>
    /// Tracks what the entity can see, responsible for dispatching entity leave/enter viewrange msgs
    /// </summary>
    public sealed class VisionComponent : EntityComponent, IVisionComponent
    {
        public override int Priority => (int)ComponentPriority.Invariant;

        public const int DefaultViewRange = 15;

        /// <summary>
        /// "Can see up to n tiles".
        /// </summary>
        public int ViewRange { get; set; } = DefaultViewRange;

        public VisionComponent(IEntity parent)
            :base(parent)
        {
            
        }
        
        public bool CanSee(IEntity ent)
        {
            var us = Parent.GetTransform();
            var oth = ent.GetTransform();

            if (!us.PoE.ContainsEntity(ent.Handle))
                return false;

            if (us.Z != oth.Z)
                return false;

            var inRange = us.ChebyshevDistanceTo(oth) <= ViewRange;
            
            // use resolver if the other entity has one
            var resolver = ent.Components.Get<IVisionResolver>();
            if (resolver != null)
            {
                return resolver.CanBeSeenBy(Parent, inRange);
            }

            return inRange;
        }        

        public IEnumerable<IEntityHandle> GetVisibleEntities()
        {
            return Parent.GetTransform().Region.GetNearbyInclusive().SelectMany(e => e.Entities)
                .Where(handle => !handle.IsDead())
                .Where(handle => !handle.Equals(Parent.Handle))
                .Where(handle => CanSee(handle.Get()));
        }

        private void NotifyNearbyEntitiesOfDeath()
        {
            this.Broadcast(EntityMessage.NearbyEntityQueuedForDeath(Parent.Handle));
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case (int)MessageId.QueuedForDeath:
                {
                    NotifyNearbyEntitiesOfDeath();
                    break;
                }
            }
        }
    }
}