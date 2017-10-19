using CScape.Models.Game.Item;

namespace CScape.Models.Game.Entity.Component
{
    public interface IGroundItemComponent : IEntityComponent
    {
        ItemStack Item { get; }

        /// <summary>
        /// How many milliseconds need to pass for the item to despawn.
        /// </summary>
        long DespawnsAfterMs { get; set; }

        long DroppedForMs { get; }

        void UpdateAmount(int newAmount);
    }
}