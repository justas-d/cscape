using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Message;
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
                    itemMsg.GetItem().Id.OnAction(Parent, (int) itemMsg.Type);
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
