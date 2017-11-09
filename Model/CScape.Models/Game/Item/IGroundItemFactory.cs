using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using JetBrains.Annotations;

namespace CScape.Models.Game.Item
{
    /// <summary>
    /// Defines a factory which create entities that have a valid IGroundItemComponent component attached to them.
    /// </summary>
    public interface IGroundItemFactory
    {
        /// <summary>
        /// Creates an entity and ensures that it has:
        /// * An IGroundItemComponent for <see cref="stack"/> tuned for being dropped by player <see cref="player"/>
        /// </summary>
        /// <param name="stack">The item stack which will be passed to the ground item.</param>
        /// <param name="player">The player who is the parent of the item (or has just simply dropped it).</param>
        /// <param name="name">The name of the entity.</param>
        [NotNull]
        IEntityHandle CreatePlayerDrop(ItemStack stack, [NotNull] IPlayerComponent player, string name);

        /// <summary>
        /// Creates an entity and ensures that it has:
        /// * An IGroundItemComponent for <see cref="stack"/>.
        /// </summary>
        /// <param name="stack">The item stack which will be passed to the ground item.</param>
        /// <param name="name">The name of the entity.</param>
        [NotNull]
        IEntityHandle Create(ItemStack stack, string name);
    }
}