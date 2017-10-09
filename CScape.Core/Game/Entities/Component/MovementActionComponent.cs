using CScape.Core.Game.Entities.Interface;
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

        public override void ReceiveMessage(EntityMessage msg)
        {
            switch (msg.Event)
            {
                case EntityMessage.EventType.ArrivedAtDestination:
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
                case EntityMessage.EventType.Teleport:
                case EntityMessage.EventType.StopMovingAlongMovePath:
                {
                    CurrentAction = null;
                    break;
                }
            }
        }
    }
}