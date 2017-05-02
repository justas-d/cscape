namespace CScape.Game.Entity
{
    public interface IDirectionsProvider
    {
        (sbyte x, sbyte y) GetNextDir();
        bool IsDone();
        void Dispose();
    }
}