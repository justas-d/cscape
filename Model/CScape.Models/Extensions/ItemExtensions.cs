using CScape.Models.Game.Entity;
using CScape.Models.Game.Item;

namespace CScape.Models.Extensions
{
    public static class ItemExtensions
    {
        public static long GetOverflow(this IItemDefinition def, long uncheckedAmount)
            => uncheckedAmount > def.MaxAmount ? uncheckedAmount - def.MaxAmount : 0;

        public static (int? existingIdx, int? emptyIdx) GetExistingOrEmptyIdx(
            this IItemContainer container, int id)
        {
            int? emptyIdx = null;
            int? existingIdx = null;

            for (var i = 0; i < container.Provider.Count; i++)
            {
                // handle empty items, store the first index we find just in case.
                var item = container.Provider[i];
                if (item.IsEmpty())
                {
                    if (emptyIdx == null)
                        emptyIdx = i;
                    continue;
                }

                // compare id's
                if (item.Id.ItemId == id)
                {
                    if (!item.IsFull())
                    {
                        // we found an existing item, set the existing item index and gtfo out of the loop.
                        existingIdx = i;
                        break;
                    }
                }
            }

            return (existingIdx, emptyIdx);
        }
    }
}