using CScape.Models.Game.Item;

namespace CScape.Models.Game.Entity.Component
{
    /// <summary>
    /// Defines a component which represents an item on the ground in the game world.
    /// </summary>
    public interface IGroundItemComponent : IEntityComponent
    {
        /// <summary>
        /// The stack if items which is on the ground in the world.
        /// </summary>
        ItemStack Item { get; }

        /// <summary>
        /// How many milliseconds need to pass in order for the item to despawn.
        /// </summary>
        long DespawnsAfterMs { get; set; }

        /// <summary>
        /// An accummulator, holding the value of milliseconds for which this item has been on the ground.
        /// </summary>
        long DroppedForMs { get; }

        /// <summary>
        /// Updates the amount of items dropped.
        /// </summary>
        void UpdateAmount(int newAmount);
    }
}