using CScape.Models.Game.Item;
using JetBrains.Annotations;

namespace CScape.Models.Game.Entity.Component
{
    /// <summary>
    /// Defines a component which exposes the entity's inventory.
    /// </summary>
    public interface IInventoryComponent : IEntityComponent
    {
        [NotNull]
        IItemContainer Inventory { get; }
    }
}