namespace CScape.Core.Game.Entity
{
    public interface IFacingData
    {
        short SyncX { get; }
        short SyncY { get; }

        int RawX { get; }
        int RawY { get; }
    }
}