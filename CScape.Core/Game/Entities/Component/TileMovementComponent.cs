using CScape.Core.Game.Entity;
using CScape.Core.Game.World;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Component
{
    public sealed class TileMovementComponent : EntityComponent
    {
        private bool _isDirectionProviderNew;
        [CanBeNull] private IDirectionsProvider _directions;

        [CanBeNull]
        public IDirectionsProvider Directions
        {
            get => _directions;
            set
            {
                if (_directions != null)
                    CancelMovingAlongPath();

                if (value == null)
                {
                    _directions = null;
                }
                else
                {
                    _isDirectionProviderNew = true;
                    _directions = value;
                }
            }
        }

        /// <summary>
        /// The direction deltas in which this entity last moved successfully.
        /// </summary>
        public DirectionDelta LastMovedDirection { get; private set; }

        public override int Priority { get; }

        public bool IsRunning { get; set; }

        public TileMovementComponent(Entity parent)
            :base(parent)
        {
        }

        private void CancelMovingAlongPath()
        {
            Directions = null;

            Parent.SendMessage(new EntityMessage(
                this, EntityMessage.EventType.StopMovingAlongMovePath, null));
        }

        private void ProcessMovement()
        {
            if (Directions == null)
                return;

            // get first movement since we've got an active direction
            var d1 = Directions.GetNextDir(Parent);
            var d2 = DirectionDelta.Noop;

            // handle running only if we actually have more direction data
            // we use IsDoneOffset in order to mimic the walking done by the first call to GetNextDir
            if (IsRunning && !Directions.IsDoneOffset(Parent, d1.X, d1.Y, 0))
            {
                // update d2 since we're running and we actually have new data in d2
                d2 = Directions.GetNextDirOffset(Parent, d1.X, d1.Y, 0);
            }

            // swap places if d1 is noop but d2 isn't.
            if (d1.IsNoop() && !d2.IsNoop())
            {
                var buf = d1;
                d1 = d2;
                d2 = buf;
            }

            // both ops are noops, no work to be done there.
            if (d1.IsNoop() && d2.IsNoop())
                return;

            // TODO : check collision (with size) and then clamp movement

            // set last moved direction
            if (!d2.IsNoop())
            {
                LastMovedDirection = d2;
            }
            else if(!d1.IsNoop())
            {
                LastMovedDirection = d1;
            }
            
            // notify entity of movement.
            Parent.SendMessage(
                new EntityMessage(
                    this, 
                    EntityMessage.EventType.Move, 
                    new MovementMetadata(d1, d2)));
        }

        private void Update()
        {
            if (_isDirectionProviderNew)
            {
                // we just received a new direction provider, let's send a begin move path message.
                Parent.SendMessage(
                    new EntityMessage(this, EntityMessage.EventType.BeginMovePath, null));

                _isDirectionProviderNew = false;
            }

            // verify whether we have an active direction provider
            if (Directions != null)
            {
                if (Directions.IsDone(Parent))
                {
                    // we're done moving. send the message
                    Parent.SendMessage(
                        new EntityMessage(this, EntityMessage.EventType.ArrivedAtDestination, null));

                    // we're done with the Directions provider, dispose it.
                    Directions = null;
                }
                else
                {
                    // we have a provider and it's not done. Process it's movement.
                    ProcessMovement();
                }
            }            
        }

        public override void ReceiveMessage(EntityMessage msg)
        {
            switch (msg.Event)
            {
                case EntityMessage.EventType.FrameUpdate:
                {
                    Update();
                    break;
                }
                case EntityMessage.EventType.Teleport:
                {
                    CancelMovingAlongPath();
                    break;
                }
            }
        }
    }
}