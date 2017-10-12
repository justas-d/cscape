using CScape.Core.Game.Entities.MovementAction;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Component
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

        public override void ReceiveMessage(GameMessage msg)
        {
            switch (msg.Event)
            {
                case GameMessage.Type.ArrivedAtDestination:
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
                case GameMessage.Type.Teleport:
                case GameMessage.Type.StopMovingAlongMovePath:
                {
                    CurrentAction = null;
                    break;
                }
            }
        }
    }
}