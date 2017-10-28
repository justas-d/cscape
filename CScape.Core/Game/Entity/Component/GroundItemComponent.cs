using System;
using System.Diagnostics;
using CScape.Core.Game.Entity.Message;
using CScape.Models;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Item;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Component
{
    [RequiresComponent(typeof(VisionComponent))]
    public class GroundItemComponent : EntityComponent, IGroundItemComponent
    {
        [CanBeNull]
        private readonly Action<GroundItemComponent> _destroyCallback;
        public override int Priority => (int)ComponentPriority.GroundItemComponent;
    

        public ItemStack Item { get; private set; }

        /// <summary>
        /// How many milliseconds need to pass for the item to despawn.
        /// </summary>
        public long DespawnsAfterMs { get; set; } = 60 * 6 * 1000;

        public long DroppedForMs { get; private set; }

        public GroundItemComponent(
            [NotNull] IEntity parent,
            ItemStack item,
            [CanBeNull] Action<GroundItemComponent> destroyCallback) : base(parent)
        {
            Debug.Assert(!Item.IsEmpty());
            _destroyCallback = destroyCallback ?? throw new ArgumentNullException(nameof(destroyCallback));
            Item = item;
        }

        protected virtual void Update()
        {
            DroppedForMs += Parent.Server.Services.ThrowOrGet<IMainLoop>().GetDeltaTime();

            // handle despawning
            if (DroppedForMs >= DespawnsAfterMs)
            {
                _destroyCallback?.Invoke(this);
            }
                
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case SysMessage.FrameUpdate:
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

            Parent.AssertGetVision().Broadcast(GroundItemMessage.AmountChange(old, Item, this));
        }
    }
}