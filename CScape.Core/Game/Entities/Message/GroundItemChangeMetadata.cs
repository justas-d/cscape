using CScape.Core.Game.Items;

namespace CScape.Core.Game.Entities.Message
{
    public sealed class GroundItemChangeMetadata
    {
        public GroundItemChangeMetadata(ItemStack before, ItemStack after)
        {
            Before = before;
            After = after;
        }

        public ItemStack Before { get; }
        public ItemStack After { get; }
    }
}