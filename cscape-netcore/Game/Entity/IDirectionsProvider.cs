namespace CScape.Game.Entity
{
    public interface IDirectionsProvider
    {
        // todo : IDirectionsProvider should be able to return a noop, MovementController should handle it but IDirectionsProvider should still exist and be able to provide further directions.
        (sbyte, sbyte) GetNextDir();
        bool IsDone();
        void Dispose();
    }
}