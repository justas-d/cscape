using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Item;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Component
{
    public sealed class PlayerInventoryComponent : EntityComponent, IInventoryComponent
    {
        public override int Priority { get; }

        // TODO : PlayerInventoryComponent load from DB

        [NotNull]
        public IItemContainer Inventory { get; }
        [NotNull]
        public IItemContainer Equipment { get; }
        [NotNull]
        public IItemContainer Bank { get; }


        public PlayerInventoryComponent([NotNull] Entity parent) : base(parent)
        {
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            
        }
    }
}
