namespace CScape.Core.Game.Entity
{
    public interface IDirectionsProvider
    {
        (sbyte x, sbyte y) GetNextDir();
        bool IsDone();
        void Dispose();
    }
}