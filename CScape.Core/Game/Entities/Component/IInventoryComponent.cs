using CScape.Core.Game.Interface;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Component
{
    public interface IInventoryComponent : IEntityComponent
    {
        [NotNull]
        IItemContainer Inventory { get; }
    }
}