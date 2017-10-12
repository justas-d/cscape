using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CScape.Core.Game.Entities.InteractingEntity;

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

        private HashSet<EntityHandle> _seeableEntities { get; }
            = new HashSet<EntityHandle>();

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

            var us = Parent.GetTransform();
            var oth = ent.GetTransform();

            if (us.PoE.ContainsEntity(ent.Handle))
                return false;

            if (us.Z != oth.Z)
                return false;

            return Parent.GetTransform().ChebyshevDistanceTo(ent.GetTransform()) <= ViewRange;
        }

        private void Reset()
        {
            _seeableEntities.Clear();
        }

        public IEnumerable<EntityHandle> GetVisibleEntities()
        {
            return Parent.GetTransform().Region.GetNearbyInclusive().SelectMany(e => e.Entities)
                .Where(handle => !handle.IsDead())
                .Where(handle => CanSee(handle.Get()));
        }

        private void Update()
        {
            // reset interacting entity if it's out of range
            var t = Parent.GetTransform();
            if (t.InteractingEntity.Entity != null && !CanSee(t.InteractingEntity.Entity))
            {
                t.SetInteractingEntity(NullInteractingEntity.Instance);
            }

            // handle visual messages

            // remove entities which we cannot see anymore
            // HACK : send message inside of predicate might not be a good design choice.
            _seeableEntities.RemoveWhere(e =>
            {
                void SendDeleteMsg(EntityHandle ent)
                {
                    Parent.SendMessage(
                        new GameMessage(
                            this, GameMessage.Type.EntityLeftViewRange, ent));
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
                    Parent.SendMessage(
                        new GameMessage(
                            this, GameMessage.Type.EntityEnteredViewRange, handle));
                }
            }
        }

        private void GC()
        {
            _seeableEntities.RemoveWhere(e => e.IsDead());
        }

        public override void ReceiveMessage(GameMessage msg)
        {
            switch (msg.Event)
            {
                case GameMessage.Type.NetworkReinitialize:
                {
                    Reset();
                    break;
                }
                case GameMessage.Type.FrameUpdate:
                {
                    Update();
                    break;
                }
                case GameMessage.Type.GC:
                {
                    GC();
                    break;
                }
            }
        }
    }
}