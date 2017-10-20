namespace CScape.Models.Game.Message
{
    public static class System
    {
        /* Sent whenever the entity is being destroyed */
        public const int DestroyEntity = int.MinValue + 1;
        /* Frame has ended, used for resets */
        public const int FrameEnd = int.MinValue + 2;
        /* Time to do update logic */
        public const int FrameUpdate = int.MinValue + 3;
        /* Collect entity garbage */
        public const int GC = int.MinValue + 4;
    }
}
