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
    /// Tracks what the entity can see
    /// </summary>
    public sealed class VisionComponent : EntityComponent, IVisionComponent
    {
        public const int DefaultViewRange = 15;

        public override int Priority { get; }

        private readonly HashSet<IEntityHandle> _seeableEntities= new HashSet<IEntityHandle>();

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

            if (us.PoE.ContainsEntity(ent.Handle))
                return false;

            if (us.Z != oth.Z)
                return false;

            var inRange = Parent.GetTransform().ChebyshevDistanceTo(ent.GetTransform()) <= ViewRange;
            
            // use resolver if the other entity has one
            var resolver = ent.Components.Get<IVisionResolver>();
            if (resolver != null)
            {
                return resolver.CanBeSeenBy(ent, inRange);
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
                .Where(handle => CanSee(handle.Get()));
        }

        private void Update()
        {
            // reset interacting entity if it's out of range
            var t = Parent.GetTransform();
            if (t.InteractingEntity.Entity != null && 
                !t.InteractingEntity.Entity.IsDead() 
                && !CanSee(t.InteractingEntity.Entity.Get()))
            {
                t.SetInteractingEntity(NullInteractingEntity.Instance);
            }

            // handle visual messages

            // remove entities which we cannot see anymore
            // HACK : send message inside of predicate might not be a good design choice.
            _seeableEntities.RemoveWhere(e =>
            {
                void SendDeleteMsg(IEntityHandle ent)
                {
                    Parent.SendMessage(EntityMessage.LeftViewRange(ent));
                }

                if (e.IsDead())
                {
                    SendDeleteMsg(e);
                    return true;
                }

                if (!CanSee(e.Get()))
                {
                    SendDeleteMsg(e);
                    return true;
                }

                return false;
            });

            // add new entities
            foreach (var handle in GetVisibleEntities())
            {
                if (!_seeableEntities.Contains(handle))
                {
                    _seeableEntities.Add(handle);
                    Parent.SendMessage(EntityMessage.EnteredViewRange(handle));
                }
            }
        }

        private void GC()
        {
            _seeableEntities.RemoveWhere(e => e.IsDead());
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case (int)MessageId.NetworkReinitialize:
                {
                    Reset();
                    break;
                }
                case SysMessage.FrameUpdate:
                {
                    Update();
                    break;
                }
                case SysMessage.GC:
                {
                    GC();
                    break;
                }
            }
        }
    }
}