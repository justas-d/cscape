namespace CScape.Game.Interface
{
    public static class ItemHelper
    {
        public static (int, int) EmptyItem { get; } = (-1, 0);

        public static bool IsEmpty((int id, int amnt) item)
        {
            return item.id < 0 || item.amnt <= 0;
        }
    }
}