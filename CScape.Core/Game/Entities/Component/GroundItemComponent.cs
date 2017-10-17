using System;
using System.Diagnostics;
using CScape.Core.Game.Entities.Message;
using CScape.Core.Game.Items;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Component
{
    [RequiresComponent(typeof(VisionComponent))]
    public class GroundItemComponent : EntityComponent
    {
        [CanBeNull]
        private readonly Action<GroundItemComponent> _destroyCallback;
        public override int Priority { get; }

        public ItemStack Item { get; private set; }

        private VisionComponent Vision => Parent.Components.AssertGet<VisionComponent>();

        /// <summary>
        /// How many milliseconds need to pass for the item to despawn.
        /// </summary>
        public long DespawnsAfterMs { get; set; } = 60 * 6 * 1000;

        public long DroppedForMs { get; private set; }

        public GroundItemComponent(
            [NotNull] Entity parent,
            ItemStack item,
            [CanBeNull] Action<GroundItemComponent> destroyCallback) : base(parent)
        {
            Debug.Assert(!Item.IsEmpty());
            _destroyCallback = destroyCallback ?? throw new ArgumentNullException(nameof(destroyCallback));
            Item = item;
        }

        protected virtual void Update()
        {
            DroppedForMs += Parent.Server.Loop.GetDeltaTime();

            // handle despawning
            if (DroppedForMs >= DespawnsAfterMs)
            {
                _destroyCallback?.Invoke(this);
            }
                
        }

        public override void ReceiveMessage(GameMessage msg)
        {
            switch (msg.Event)
            {
                case GameMessage.Type.FrameUpdate:
                    Update();
                    break;
            }
        }

        public void UpdateAmount(int newAmount)
        {
            if (Item.Amount == newAmount) return;
            if (0 >= newAmount) return;

            var old = Item;
            Item = new ItemStack(Item.Id, newAmount);

            Vision.Broadcast(
                new GameMessage(
                    this, GameMessage.Type.GroundItemAmountUpdate, 
                    new GroundItemChangeMetadata(old, Item, this)));
        }
    }
}