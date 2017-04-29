namespace CScape.Game.Entity
{
    public interface IMovingEntity : IEntity
    {
        Transform Position { get; }
        MovementController Movement { get; }
        (sbyte x, sbyte y) LastMovedDirection { get; set; }
        IEntity InteractingEntity { get; set; }

        void OnMoved();
    }
}