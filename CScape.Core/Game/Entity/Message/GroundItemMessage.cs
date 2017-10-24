using System;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Item;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Message
{
    public sealed class GroundItemMessage : IGameMessage
    {
        [NotNull]
        public IGroundItemComponent Item { get; }

        public ItemStack Before { get; }
        public ItemStack After { get; }
        public int EventId { get; }

        public static GroundItemMessage AmountChange(ItemStack before, ItemStack after, [NotNull] IGroundItemComponent item)
            => new GroundItemMessage(before, after ,item, MessageId.GroundItemAmountUpdate);

        private GroundItemMessage(ItemStack before, ItemStack after, [NotNull] IGroundItemComponent item, MessageId id)
        {
            Before = before;
            After = after;
            Item = item ?? throw new ArgumentNullException(nameof(item));
            EventId = (int)id;
        }

    }
}