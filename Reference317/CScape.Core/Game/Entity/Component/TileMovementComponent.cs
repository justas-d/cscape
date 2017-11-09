using System.Diagnostics;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Directions;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Directions;
using CScape.Models.Game.World;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Component
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
                    Parent.SendMessage(NotificationMessage.StopMovingAlongMovePath);

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

        public override int Priority => (int) ComponentPriority.TileMovement;        

        public bool IsRunning { get; set; }

        public TileMovementComponent(IEntity parent)
            : base(parent)
        {
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
            
            // split it into running or moving
            MoveMessage msg;
            if (IsRunning)
            {
                msg = new MoveMessage(data.Walk, data.Run);
            }
            else
            {
                msg = new MoveMessage(data.Walk, DirectionDelta.Noop);
            }

            // TODO : check collision (with size) and then clamp movement

            // notify entity of movement.
            Parent.SendMessage(msg);
        }

        private void Update()
        {
            if (_isDirectionProviderNew)
            {
                Debug.Assert(_directions != null);
                
                // we just received a new direction provider, let's send a begin move path message.
                Parent.SendMessage(new BeginMovePathMessage(_directions));

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
            if (ent.Equals(Parent.Handle)) return;

            Directions = new FollowDirectionProvider(ent);
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case (int)MessageId.FrameBegin:
                {
                    Update();
                    break;
                }
                case (int)MessageId.Teleport:
                {
                    Directions = null;
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