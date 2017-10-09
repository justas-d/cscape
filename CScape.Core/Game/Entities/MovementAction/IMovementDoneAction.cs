namespace CScape.Core.Game.Entities.MovementAction
{
    /// <summary>
    /// Defines a functor that gets executed when any owning movement controller's direction provider is done.
    /// </summary>
    public interface IMovementDoneAction
    {
        void Execute();
    }
}