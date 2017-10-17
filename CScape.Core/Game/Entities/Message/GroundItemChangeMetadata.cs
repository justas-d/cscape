using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Items;

namespace CScape.Core.Game.Entities.Message
{
    public sealed class GroundItemChangeMetadata
    {
        public GroundItemComponent Item { get; }

        public GroundItemChangeMetadata(ItemStack before, ItemStack after, GroundItemComponent item)
        {
            Before = before;
            After = after;
            Item = item;
        }

        public ItemStack Before { get; }
        public ItemStack After { get; }
    }
}