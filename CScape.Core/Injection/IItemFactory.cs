using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Items;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities
{
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