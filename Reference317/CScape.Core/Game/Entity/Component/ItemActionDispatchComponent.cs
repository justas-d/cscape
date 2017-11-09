using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Component
{
    public sealed class ItemActionDispatchComponent : EntityComponent
    {
        public override int Priority => (int)ComponentPriority.ItemActionDispatchComponent;

        public ItemActionDispatchComponent([NotNull] IEntity parent) : base(parent)
        {
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case (int) MessageId.ItemAction:
                {
                    var itemMsg = msg.AsItemAction();
                    var item = itemMsg.GetItem();
                    item.Id.OnAction(Parent, itemMsg.Container, itemMsg.ItemIndexInContainer, itemMsg.Interface, item, (int)itemMsg.ItemActionType);
                    break;
                }

                case (int) MessageId.ItemOnItemAction:
                {
                    var itemMsg = msg.AsItemOnItemAction();
                    itemMsg.GetItemA().Id.UseWith(Parent, itemMsg.GetItemB());
                    break;
                }
            }
        }
    }
}
