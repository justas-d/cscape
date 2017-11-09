using System;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Item;
using JetBrains.Annotations;

namespace CScape.Models.Extensions
{
    public static class ContainerExtensions
    {
        /// <summary>
        /// Retrieves an item in the given container that's locate at the given index.
        /// </summary>
        /// <returns>An <see cref="ItemStack"/>of the item at the given index in the given container, null if that item is empty or the index is out of range.</returns>
        [CanBeNull]
        public static ItemStack GetItem(this IItemContainer container, int itemIndex)
        {
            if (0 > itemIndex || itemIndex >= container.Provider.Count)
                return null;

            var item = container.Provider[itemIndex];
            if (item.IsEmpty())
                return null;

            return item;
        }

        /// <summary>
        /// Drops an item from a given inventory as a player.
        /// </summary>
        /// <returns>On success, the handle of the dropped item. Null otherwise.</returns>
        [CanBeNull]
        public static IEntityHandle PlayerDropItem(this IItemContainer container, int itemIndex, [NotNull] IPlayerComponent player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));

            var item = container.GetItem(itemIndex);
            if (item == null)
                return null;

            var itemFactory = (IGroundItemFactory)player.Parent.Server.Services.GetService(typeof(IGroundItemFactory));
            if (itemFactory == null)
                return null;

            var itemEntityHandle = itemFactory.CreatePlayerDrop(item, player, $"Dropped item {item.Id.Name}x{item.Amount}");
            if (itemEntityHandle.IsDead())
                return null;

            var itemEntity = itemEntityHandle.Get();
            var itemTransform = itemEntity.GetTransform();
            var playerTransform = player.Parent.GetTransform();

            itemTransform.SwitchPoE(playerTransform.PoE);
            itemTransform.Teleport(playerTransform);

            container.ExecuteChangeInfo(ItemChangeInfo.Remove(itemIndex));

            return itemEntityHandle;
        }
    }
}
