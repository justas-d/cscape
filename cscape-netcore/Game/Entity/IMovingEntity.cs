namespace CScape.Game.Entity
{
    public interface IMovingObserver : IMovingEntity, IObserver
    {
        
    }

    public interface IMovingEntity : IEntity
    {
        MovementController Movement { get; }
        (sbyte x, sbyte y) LastMovedDirection { get; set; }
        IEntity InteractingEntity { get; set; }

        void OnMoved();
    }
}