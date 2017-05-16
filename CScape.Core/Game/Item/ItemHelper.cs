using CScape.Core.Game.Interface;

namespace CScape.Core.Game.Item
{
    public static class ItemHelper
    {
        public static readonly (int id, int amount) EmptyItem = (EmptyId, EmptyAmount);

        public const int EmptyId = 0;
        public const int EmptyAmount = 0;

        public static bool IsEmpty(int id, int amount) => id <= EmptyId || amount <= EmptyAmount;
        public static bool IsEmpty((int id, int amount) item) => IsEmpty(item.id, item.amount);
        public static bool IsEmptyAtIndex(this IItemProvider provider, int idx)
        {
            if (0 > idx || idx >= provider.Count) return true;
            return IsEmpty(provider.GetId(idx), provider.GetAmount(idx));
        }

        public static long CalculateOverflow(IItemDefinition def, long uncheckedAmount)
        {
            return uncheckedAmount > def.MaxAmount ? uncheckedAmount - def.MaxAmount : 0;
        }

        public static bool  ExecuteChangeInfo(IItemManager manager, ItemProviderChangeInfo info)
        {
            if (!info.IsValid)
                return false;

            if (info.Index < 0 || info.Index >= manager.Size)
                return false;

            // execute
            manager.Provider.SetId(info.Index, info.NewItemDefId);
            manager.Provider.SetAmount(info.Index, info.NewAmount);

            if (manager.Provider.IsEmptyAtIndex(info.Index))
            {
                manager.Provider.SetId(info.Index, EmptyId);
                manager.Provider.SetAmount(info.Index, EmptyAmount);
            }

            return true;
        }
    }
}