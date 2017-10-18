using CScape.Core.Game.Interface;
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

        public override void ReceiveMessage(GameMessage msg)
        {
            
        }
    }
}
