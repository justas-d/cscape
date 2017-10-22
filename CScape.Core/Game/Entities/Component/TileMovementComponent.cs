using CScape.Core.Game.Entities.Directions;
using CScape.Core.Game.Entities.Message;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Directions;
using CScape.Models.Game.Message;
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

        public override int Priority { get; }

        public bool IsRunning { get; set; }

        public TileMovementComponent(Entity parent)
            : base(parent)
        {
        }

        private void CancelMovingAlongPath()
        {
            Directions = null;

            Parent.SendMessage(NotificationMessage.StopMovingAlongMovePath);
        }

        private void ProcessMovement()
        {
            if (Directions == null)
                return;

            // get move data
            var data = Directions.GetNextDirections(Parent);

            // both ops are noops, no work to be done there.
            if (data.IsNoop())
                return;

            // TODO : check collision (with size) and then clamp movement

            // notify entity of movement.
            Parent.SendMessage(new MoveMessage(data.Walk, data.Run));
        }

        private void Update()
        {
            if (_isDirectionProviderNew)
            {
                // we just received a new direction provider, let's send a begin move path message.
                Parent.SendMessage(NotificationMessage.BeginMovePath);

                _isDirectionProviderNew = false;
            }

            // verify whether we have an active direction provider
            if (Directions != null)
            {
                if (Directions.IsDone(Parent))
                {
                    // we're done moving. send the message
                    Parent.SendMessage(NotificationMessage.ArrivedAtDestination);

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

        private void SetNewFollowTarget(IEntityHandle ent)
        {
            if (ent.IsDead()) return;
            Directions = new FollowDirectionProvider(ent);
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case SysMessage.FrameUpdate:
                {
                    Update();
                    break;
                }
                case (int)MessageId.Teleport:
                {
                    CancelMovingAlongPath();
                    break;
                }
                case (int)MessageId.NewPlayerFollowTarget:
                {
                    SetNewFollowTarget(msg.AsNewPlayerFollowTarget().Entity);
                    break;
                }
            }
        }
    }
}