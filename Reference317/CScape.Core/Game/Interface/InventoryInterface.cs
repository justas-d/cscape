using System;
using System.Collections.Generic;
using System.Linq;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Network.Packet;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Interface;
using CScape.Models.Game.Item;
using JetBrains.Annotations;

namespace CScape.Core.Game.Interface
{
    public class InventoryInterface : IItemGameInterface
    {
        public int Id { get; }


        [NotNull]
        public IItemContainer Container { get; }

        private readonly HashSet<int> _dirtyBuffer = new HashSet<int>();

        public InventoryInterface(
            int id, [NotNull] IItemContainer container)
        {
            Id = id;
            Container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public bool Equals(IGameInterface other) => Id == other.Id;

        public void ShowForEntity(IEntity entity)
        {
            entity.SendMessage(InterfaceMessage.Show(this, new MassSendInterfaceItemsPacket(Id, Container)));
        }

        public void CloseForEntity(IEntity entity)
        {
            entity.SendMessage(InterfaceMessage.Close(this, new ClearItemInterfacePacket(Id)));
        }

        public void UpdateForEntity(IEntity entity)
        {
            // only update dirty items if we have any
            if (_dirtyBuffer.Any())
            {
                entity.SendMessage(InterfaceMessage.Update(this, new UpdateInterfaceItemPacket(this, Container, _dirtyBuffer)));
                _dirtyBuffer.Clear();
            }
        }

        private void HandleItemChange(ItemChangeInfo info)
        {
            _dirtyBuffer.Add(info.Index);
        }

        private bool IsOurContainer(IItemContainer other)
            => ReferenceEquals(other, Container);

        public void ReceiveMessage(IEntity entity, IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case (int)MessageId.ItemChange:
                {
                    var data = msg.AsItemChange();
                    if (IsOurContainer(data.Container))
                        HandleItemChange(data.Info);

                    break;
                }
            }
        }
    }
}
