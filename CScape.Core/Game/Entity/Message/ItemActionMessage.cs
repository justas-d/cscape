using System;
using CScape.Core.Game.Item;
using CScape.Models.Game.Interface;
using CScape.Models.Game.Item;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Message
{
    public sealed class ItemActionMessage : IGameMessage
    {
        public ItemActionType ItemActionType { get; }
        [NotNull]
        public IItemContainer Container { get; }

        public InterfaceMetadata Interface { get; }

        public int ItemIndexInContainer { get; }

        public ItemActionMessage(
            ItemActionType itemActionType, 
            [NotNull] IItemContainer container,
            InterfaceMetadata @interface, 
            int itemIndexInContainer)
        {
            if (0 > itemIndexInContainer || itemIndexInContainer >= container.Provider.Count) throw new ArgumentOutOfRangeException(nameof(itemIndexInContainer));
            ItemActionType = itemActionType;
            Container = container ?? throw new ArgumentNullException(nameof(container));
            Interface = @interface;
            ItemIndexInContainer = itemIndexInContainer;
        }

        public int EventId => (int)MessageId.ItemAction;

        public ItemStack GetItem() => Container.Provider[ItemIndexInContainer];
    }
}