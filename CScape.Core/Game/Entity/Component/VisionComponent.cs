using System.Collections.Generic;
using System.Linq;
using CScape.Core.Extensions;
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
    /// Tracks what the entity can see
    /// </summary>
    public sealed class VisionComponent : EntityComponent, IVisionComponent
    {
        public const int DefaultViewRange = 15;

        public override int Priority => (int)ComponentPriority.VisionComponent;

        private readonly HashSet<IEntityHandle> _seeableEntities= new HashSet<IEntityHandle>();
        private readonly HashSet<IEntityHandle> _deleteQueue = new HashSet<IEntityHandle>();

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

        private void Reset()
        {
            _seeableEntities.Clear();
        }

        public IEnumerable<IEntityHandle> GetVisibleEntities()
        {
            return Parent.GetTransform().Region.GetNearbyInclusive().SelectMany(e => e.Entities)
                .Where(handle => !handle.IsDead())
                .Where(handle => !handle.Equals(Parent.Handle))
                .Where(handle => CanSee(handle.Get()));
        }

        private void Update()
        {
            // try add new entities
            foreach (var handle in GetVisibleEntities())
                TryAddEntityToSeeables(handle);

            // reset interacting entity if it's out of range
            var t = Parent.GetTransform();

            if (t.InteractingEntity.Entity != null && 
                !t.InteractingEntity.Entity.IsDead() 
                && !CanSee(t.InteractingEntity.Entity.Get()))
            {
                t.SetInteractingEntity(NullInteractingEntity.Instance);
            }

            // perform gc
            RemoveAnyEntitiesWeCannotSee();
        }

        private void RemoveAnyEntitiesWeCannotSee()
        {
            // append to delete queue
            foreach (var handle in _seeableEntities)
            {
                if (handle.IsDead())
                    _deleteQueue.Add(handle);
                else if (!CanSee(handle.Get()))
                    _deleteQueue.Add(handle);
            }

            foreach (var handle in _deleteQueue)
                TryDeleteEntityFromSeeables(handle);

            _deleteQueue.Clear();
        }

        private bool TryAddEntityToSeeables(IEntityHandle handle)
        {
            if (handle.IsDead() || handle.IsQueuedForDeath())
                return false;

            if (!_seeableEntities.Add(handle))
                return false;

            Parent.SendMessage(EntityMessage.EnteredViewRange(handle));
            return true;
        }

        private bool TryDeleteEntityFromSeeables(IEntityHandle handle)
        {
            if (!_seeableEntities.Remove(handle))
                return false;

            Parent.SendMessage(EntityMessage.LeftViewRange(handle));
            return true;
        }

        private void NotifyNearbyEntitiesOfDeath()
        {
            this.Broadcast(EntityMessage.NearbyEntityQueuedForDeath(Parent.Handle));
        }

        private void CatchNearbyEntityDying(EntityMessage msg)
        {
            TryDeleteEntityFromSeeables(msg.Entity);
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case (int) MessageId.NearbyEntityQueuedForDeath:
                {
                    CatchNearbyEntityDying(msg.AsNearbyEntityQueuedForDeath());
                    break;
                }
                case (int) MessageId.QueuedForDeath:
                {
                    NotifyNearbyEntitiesOfDeath();
                    break;
                }
                case (int)MessageId.NetworkReinitialize:
                {
                    Reset();
                    break;
                }
                case (int)MessageId.FrameBegin:
                {
                    Update();
                    break;
                }
            }
        }
    }
}