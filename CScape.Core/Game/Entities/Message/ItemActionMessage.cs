using System;
using CScape.Core.Game.Item;
using CScape.Models.Game.Interface;
using CScape.Models.Game.Item;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Message
{
    public sealed class ItemActionMessage : IGameMessage
    {
        public ItemActionType Type { get; }
        [NotNull]
        public IItemContainer Container { get; }

        public InterfaceMetadata Interface { get; }

        public int Index { get; }

        public ItemActionMessage(
            ItemActionType type, 
            [NotNull] IItemContainer container,
            InterfaceMetadata @interface, 
            int index)
        {
            Type = type;
            Container = container ?? throw new ArgumentNullException(nameof(container));
            Interface = @interface;
            Index = index;
        }

        public int EventId => MessageId.ItemAction;
    }
}