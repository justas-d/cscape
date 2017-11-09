using CScape.Core.Game.Item;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Interface;
using CScape.Models.Game.Item;

namespace CScape.Core.Json
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

            public void OnAction(IEntity parentEntity, IItemContainer itemsContainer, int itemIndexInContainer,
                InterfaceMetadata containerInterfaceMetadata, ItemStack item, int actionId)
            {
                // drop logic
                if (actionId == (int)ItemActionType.Drop)
                {
                    var player = parentEntity.GetPlayer();
                    if (player != null)
                    {
                        itemsContainer.PlayerDropItem(itemIndexInContainer, player);
                        return;
                    }
                }
                
                parentEntity.SystemMessage($"Unimplemented item {ItemId} action {actionId}");
            }
        }

        public IItemDefinition Get(int id)
        {
            if (id == 0)
                return ItemStack.EmptyItem;
            
            return new UnimplementedItem(id);
        }
    }
}
