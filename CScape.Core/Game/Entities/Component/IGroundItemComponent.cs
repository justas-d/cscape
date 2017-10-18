using CScape.Core.Game.Items;

namespace CScape.Core.Game.Entities.Component
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