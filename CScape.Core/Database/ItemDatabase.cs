using CScape.Models.Game.Entity;
using CScape.Models.Game.Item;

namespace CScape.Core.Database
{
    public sealed class ItemDatabase
    {
        private sealed class UnimplementedItem : IItemDefinition
        {
            public bool Equals(IItemDefinition other) => other.ItemId == ItemId;

            public int ItemId { get; }
            public string Name => "Unimplemented item";
            public int MaxAmount => int.MaxValue;
            public bool IsTradable => false;
            public float Weight => 0;
            public bool IsNoted => false;
            public int NoteSwitchId => -1;

            public UnimplementedItem(int id)
            {
                ItemId = id;
            }

            public void UseWith(IEntity entity, ItemStack other)
            {
                entity.SystemMessage($"Unimplemented item {ItemId} used with {other.Id.ItemId} {other.Id.ItemId}");
            }

            public void OnAction(IEntity entity, int actionId)
            {
                entity.SystemMessage($"Unimplemented item {ItemId} action {actionId}");
            }
        }


        public IItemDefinition Get(int id)
        {
            return new UnimplementedItem(id);
        }
    }
}
