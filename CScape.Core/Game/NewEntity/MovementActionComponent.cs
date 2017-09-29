using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.NewEntity
{
    public sealed class MovementActionComponent : IEntityComponent
    {
        public Entity Parent { get; }

        [CanBeNull]
        public IMovementDoneAction CurrentAction { get; set; }

        public MovementActionComponent(Entity parent)
        {
            Parent = parent;
        }
        
        public void Update(IMainLoop loop) { }
        
        public void ReceiveMessage(EntityMessage msg)
        {
            if (msg.Event == EntityMessage.EventType.ArrivedAtDestination)
            {
                if (CurrentAction != null)
                {
                    // assign to local to allow MoveAction to reset itself in Execute()
                    var action = CurrentAction;
                    CurrentAction = null;

                    // run action
                    action.Execute();
                }
            }

            else if (msg.Event == EntityMessage.EventType.StopMovingAlongMovePath)
            {
                CurrentAction = null;
            }
        }
    }
}