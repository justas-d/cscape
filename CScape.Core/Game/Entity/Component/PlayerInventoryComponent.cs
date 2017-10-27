using System;
using CScape.Core.Game.Item;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Item;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Component
{
    public sealed class PlayerInventoryComponent : EntityComponent, IInventoryComponent
    {
        public override int Priority { get; }

        public const int BackpackSize = 28;
        public const int BankSize = 500;

        IItemContainer IInventoryComponent.Inventory => Backpack;

        [NotNull]
        public ListItemContainer Backpack { get; }
        [NotNull]
        public PlayerEquipmentContainer Equipment { get; }
        [NotNull]
        public ListItemContainer Bank { get; }


        public PlayerInventoryComponent(
            [NotNull] IEntity parent, 
            [NotNull] ListItemContainer backpack, 
            [NotNull] PlayerEquipmentContainer equipment, 
            [NotNull] ListItemContainer bank) : base(parent)
        {
            Backpack = backpack ?? throw new ArgumentNullException(nameof(backpack));
            Equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
            Bank = bank ?? throw new ArgumentNullException(nameof(bank));
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            
        }
    }
}
