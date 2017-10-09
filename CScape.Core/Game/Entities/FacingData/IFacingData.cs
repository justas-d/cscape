namespace CScape.Core.Game.Entities.FacingData
{
    public interface IFacingData
    {
        short SyncX { get; }
        short SyncY { get; }

        int RawX { get; }
        int RawY { get; }
    }
}