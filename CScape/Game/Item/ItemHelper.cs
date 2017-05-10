using CScape.Game.Interface;

namespace CScape.Game.Item
{
    public static class ItemHelper
    {
        public const int EmptyId = -1;
        public const int EmptyAmount = 0;

        public static bool IsEmpty(int id, int amount) => id < 0 || amount <= 0;
        public static bool IsEmpty((int id, int amount) item) => IsEmpty(item.id, item.amount);
        public static bool IsEmptyAtIndex(this IItemProvider provider, int idx)
        {
            if (0 > idx || idx >= provider.Size) return true;
            return IsEmpty(provider.Ids[idx], provider.Amounts[idx]);
        }
    }
}