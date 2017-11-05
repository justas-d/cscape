using System;
using System.Diagnostics;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Item;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Component
{
    [RequiresComponent(typeof(VisionComponent))]
    public class GroundItemComponent : EntityComponent, IGroundItemComponent
    {
        [CanBeNull]
        private readonly Action<GroundItemComponent> _onDestroy;
        public override int Priority => (int)ComponentPriority.GroundItemComponent;
    

        public ItemStack Item { get; private set; }

        /// <summary>
        /// How many milliseconds need to pass for the item to despawn.
        /// </summary>
        public long DespawnsAfterMs { get; set; } = 60 * 6 * 1000;

        public long DroppedForMs { get; private set; }

        public GroundItemComponent(
            [NotNull] IEntity parent,
            [NotNull] ItemStack item,
            [CanBeNull] Action<GroundItemComponent> onDestroy) : base(parent)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            Debug.Assert(!item.IsEmpty());

            _onDestroy = onDestroy;
            Item = item;
        }

        protected virtual void Update()
        {
            DroppedForMs += Loop.GetDeltaTime();

            // handle despawning
            if (DroppedForMs >= DespawnsAfterMs)
                Parent.Handle.Destroy();
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case (int)MessageId.FrameBegin:
                {
                    Update();
                    break;
                }
                case (int)MessageId.QueuedForDeath:
                {
                    _onDestroy?.Invoke(this);
                    break;
                }
            }
        }

        public void UpdateAmount(int newAmount)
        {
            if (Item.Amount == newAmount) return;
            if (0 >= newAmount) return;

            var old = Item;
            Item = new ItemStack(Item.Id, newAmount);

            Parent.AssertGetVision().Broadcast(GroundItemMessage.AmountChange(old, Item, this));
        }
    }
}