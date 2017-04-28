namespace CScape.Game.Entity
{
    public interface IDirectionsProvider
    {
        (sbyte, sbyte) GetNextDir();
        bool IsDone();
        void Dispose();
    }
}