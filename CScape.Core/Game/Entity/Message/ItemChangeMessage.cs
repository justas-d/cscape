using System;
using CScape.Models.Game.Item;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Message
{
    public sealed class ItemChangeMessage : IGameMessage
    {
        [NotNull]
        public IItemContainer Container { get; }
        public ItemChangeInfo Info { get; }

        public static ItemChangeMessage EquipmentChange([NotNull] IItemContainer container, ItemChangeInfo info)
            => new ItemChangeMessage(container, info, MessageId.EquipmentChange);

        public static ItemChangeMessage InventoryChange([NotNull] IItemContainer container, ItemChangeInfo info)
            => new ItemChangeMessage(container, info, MessageId.ItemChange);

        private ItemChangeMessage([NotNull] IItemContainer container, ItemChangeInfo info, MessageId id)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
            Info = info;
            EventId = (int)id;
        }

        public int EventId { get; }
    }
}
