using System;
using CScape.Core.Game.World;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    public class GroundItem : WorldEntity
    {
        // sync vars
        public int OldAmount { get; private set; }
        public bool NeedsAmountUpdate { get; private set; }

        public int ItemId { get; }
        public int ItemAmount { get; private set; }
        public Player DroppedBy { get; }

        /// <summary>
        /// How many ms need to pass after the creation of the item in order for it to become public.
        /// </summary>
        public long BecomesPublicAfterMs { get; } = 30 * 1000; // todo: 2 minutes

        /// <summary>
        /// How many milliseconds need to pass for the item to despawn.
        /// </summary>
        public long DespawnsAfterMs { get; } = 60 * 1 * 1000; // todo: 6 minutes

        /// <summary>
        /// Whether this item can be seen by everybody, not just by the player who dropped it.
        /// </summary>
        public bool IsPublic { get; private set; }

        public GroundItem(
            [NotNull] IServiceProvider services, 
            (int id, int amount) item,
            IPosition pos, Player droppedBy, PlaneOfExistance poe = null) 
            : base(services)
        {
            ItemId = item.id;
            ItemAmount = item.amount;

            DroppedBy = droppedBy;

            var t = new ServerTransform(this, pos, poe);
            Transform = t;

            services.ThrowOrGet<IMainLoop>().Item.Enqueue(this);
        }

        private long _droppedForMs;

        public override void Update(IMainLoop loop)
        {
            if (IsDestroyed)
                return;

            // reset update flags
            NeedsAmountUpdate = false;

            // accumulate alive time
            _droppedForMs += loop.DeltaTime + loop.ElapsedMilliseconds;

            // handle the item going public
            if (!IsPublic)
            {
                if (_droppedForMs >= BecomesPublicAfterMs)
                    IsPublic = true;
            }

            // handle despawning
            if (_droppedForMs >= DespawnsAfterMs)
                // keep the item in the update loop for 1 more tick 
                // after being destroyed so that ground item sync machines can
                // see that this item needs to be removed.
                Destroy(); 

            loop.Item.Enqueue(this);
        }

        public void UpdateAmount(int newAmount)
        {
            if (ItemAmount == newAmount) return;
            if (0 >= newAmount) return;

            OldAmount = ItemAmount;

            ItemAmount = newAmount;
            NeedsAmountUpdate = true;
        }
    }
}