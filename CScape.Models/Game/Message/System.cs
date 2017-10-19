namespace CScape.Models.Game.Message
{
    public static class System
    {
        public const int DestroyEntity = int.MinValue + 1;
        public const int FrameEnd = int.MinValue + 2;
        public const int FrameUpdate = int.MinValue + 3;
        public const int GC = int.MinValue + 4;
        public const int NewSystemMessage = int.MinValue + 5;
    }
}
