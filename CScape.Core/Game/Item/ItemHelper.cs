using CScape.Core.Game.Interface;

namespace CScape.Core.Game.Item
{
    public static class ItemHelper
    {
        public const int EmptyId = 0;
        public const int EmptyAmount = 0;

        public static bool IsEmpty(int id, int amount) => id <= EmptyId || amount <= EmptyAmount;
        public static bool IsEmpty((int id, int amount) item) => IsEmpty(item.id, item.amount);
        public static bool IsEmptyAtIndex(this IItemProvider provider, int idx)
        {
            if (0 > idx || idx >= provider.Size) return true;
            return IsEmpty(provider.Ids[idx], provider.Amounts[idx]);
        }
    }
}