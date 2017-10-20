using System;
using CScape.Core.Game.Entities.Component;
using CScape.Models.Game.Item;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Message
{
    public sealed class GroundItemMessage : IGameMessage
    {
        [NotNull]
        public GroundItemComponent Item { get; }

        public ItemStack Before { get; }
        public ItemStack After { get; }
        public int EventId { get; }

        public static GroundItemMessage AmountChange(ItemStack before, ItemStack after, [NotNull] GroundItemComponent item)
            => new GroundItemMessage(before, after ,item, MessageId.GroundItemAmountUpdate);

        private GroundItemMessage(ItemStack before, ItemStack after, [NotNull] GroundItemComponent item, int id)
        {
            Before = before;
            After = after;
            Item = item ?? throw new ArgumentNullException(nameof(item));
            EventId = id;
        }

    }
}