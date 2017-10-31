using System;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Item;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Component
{
    /// <summary>
    /// Item component for player dropped items that correctly handles vision resolving for the player who dropped the item.
    /// </summary>
    public class PlayerDroppedItemComponent : GroundItemComponent, IVisionResolver
    {
        [NotNull]
        public string DroppedBy { get; }

        /// <summary>
        /// How many ms need to pass after the creation of the item in order for it to become public.
        /// </summary>
        public long BecomesPublicAfterMs { get; set; } = 60 * 2 * 1000;

        /// <summary>
        /// Whether this item can be seen by everybody, not just by the player who dropped it.      
        /// </summary>
        public bool IsPublic { get; set; }

        public PlayerDroppedItemComponent(
            [NotNull] IEntity parent,
            ItemStack item,
            [CanBeNull] Action<GroundItemComponent> onDestroy,
            [NotNull] string droppedBy) : base(parent, item, onDestroy)
        {
            DroppedBy = droppedBy ?? throw new ArgumentNullException(nameof(droppedBy));
        }

        protected override void Update()
        {
            base.Update();
            if (!IsPublic)
            {
                if (DroppedForMs >= BecomesPublicAfterMs)
                    IsPublic = true;
            }
        }

        public bool CanBeSeenBy(IEntity ent, bool inRange)
        {
            if (!inRange) return false;
            if (IsPublic) return true;

            var player = ent.GetPlayer();
            if (player == null) return false;

            return player.Equals(DroppedBy);
        }
    }
}