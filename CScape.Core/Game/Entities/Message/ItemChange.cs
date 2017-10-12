using CScape.Core.Game.Interface;

namespace CScape.Core.Game.Entities.Message
{
    public sealed class ItemChange
    {
        public IItemContainer Container { get; }
        public ItemChangeInfo Info { get; }

        public ItemChange(IItemContainer container, ItemChangeInfo info)
        {
            Container = container;
            Info = info;
        }
    }
}
