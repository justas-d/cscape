namespace CScape.Core.Game.Entity
{
    /// <summary>
    /// Defines a functor that gets executed when any owning movement controller's direction provider is done.
    /// </summary>
    public interface IMovementDoneAction
    {
        /// <summary>
        /// Executes the movement done action. Should only be called when the movement is finished.
        /// It is frame unsafe, meaning all entity variables and preconditions must be checked and verified.
        /// </summary>
        void Execute();
    }
}