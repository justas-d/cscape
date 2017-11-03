using System.Collections.Generic;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Component
{
    /// <summary>
    /// Responsible for tracking nearby entities and dispatching leave/enter viewrange messages
    /// </summary>
    [RequiresComponent(typeof(VisionComponent))]
    public sealed class NearbyEntityWatcherComponent : EntityComponent
    {
        public override int Priority => (int)ComponentPriority.EntityWatcher;

        private readonly HashSet<IEntityHandle> _seeableEntities = new HashSet<IEntityHandle>();
        private readonly HashSet<IEntityHandle> _deleteQueue = new HashSet<IEntityHandle>();

        public NearbyEntityWatcherComponent([NotNull] IEntity parent) : base(parent)
        {
        }

        
        private void Reset()
        {
            _seeableEntities.Clear();
        }

        private void Update()
        {
            var vision = Parent.AssertGetVision();

            // try add new entities
            foreach (var handle in vision.GetVisibleEntities())
                TryAddEntityToSeeables(handle);

            // perform gc
            RemoveAnyEntitiesWeCannotSee();
        }
        private void RemoveAnyEntitiesWeCannotSee()
        {
            var vision = Parent.AssertGetVision();

            // append to delete queue
            foreach (var handle in _seeableEntities)
            {
                if (handle.IsDead())
                    _deleteQueue.Add(handle);
                else if (!vision.CanSee(handle.Get()))
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
                case (int) MessageId.NetworkReinitialize:
                {
                    Reset();
                    break;
                }
                case (int) MessageId.FrameBegin:
                {
                    Update();
                    break;
                }
            }
        }
    }
}