namespace CScape.Core.Game.Entities.Message
{
    public sealed class TeleportMessageData
    {
        public (int x, int y, int z) OldPos { get; }
        public (int x, int y, int z) NewPos { get; }

        public TeleportMessageData(
            (int x, int y, int z) oldPos,
            (int x, int y, int z) newPos)
        {
            OldPos = oldPos;
            NewPos = newPos;
        }
    }
}