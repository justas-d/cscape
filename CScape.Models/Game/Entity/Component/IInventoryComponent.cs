using CScape.Models.Game.Item;
using JetBrains.Annotations;

namespace CScape.Models.Game.Entity.Component
{
    /// <summary>
    /// 
    /// </summary>
    public interface IInventoryComponent : IEntityComponent
    {
        [NotNull]
        IItemContainer Inventory { get; }
    }
}