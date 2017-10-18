using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Items;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities
{
    /// <summary>
    /// Defines a factory which create entities that have a valid IGroundItemComponent component attached to them.
    /// </summary>
    public interface IItemFactory
    {
        [NotNull]
        IEntitySystem System { get; }

        [NotNull]
        EntityHandle CreatePlayerDrop(ItemStack stack, [NotNull] PlayerComponent player, string name);

        [NotNull]
        EntityHandle Create(ItemStack stack, string name);
    }
}