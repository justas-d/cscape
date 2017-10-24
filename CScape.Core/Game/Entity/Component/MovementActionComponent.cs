using CScape.Core.Game.Entity.Message;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Component
{
    public sealed class MovementActionComponent : EntityComponent
    {
        // TODO : MovementActionComponent  priority
        public override int Priority { get; }

        [CanBeNull]
        public IMovementDoneAction CurrentAction { get; set; }

        public MovementActionComponent(Entity parent)
            : base(parent)
        {

        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case (int)MessageId.ArrivedAtDestination:
                {
                    if (CurrentAction != null)
                    {
                        // assign to local to allow MoveAction to reset itself in Execute()
                        var action = CurrentAction;
                        CurrentAction = null;

                        // run action
                        action.Execute();
                    }
                    break;
                }
                case (int)MessageId.Teleport:
                case (int)MessageId.StopMovingAlongMovePath:
                {
                    CurrentAction = null;
                    break;
                }
            }
        }
    }
}