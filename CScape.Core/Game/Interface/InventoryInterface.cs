using System;
using System.Collections.Generic;
using System.Linq;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Game.Interface;
using CScape.Core.Network;
using CScape.Core.Network.Packet;
using JetBrains.Annotations;

namespace CScape.Core.Game.Interfaces
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

        public IEnumerable<IPacket> GetShowPackets()
        {
            yield return new MassSendInterfaceItemsPacket(Id, Container);
        }

        public IEnumerable<IPacket> GetClosePackets()
        {
            yield return new ClearItemInterfacePacket(Id);
        }
        
        public IEnumerable<IPacket> GetUpdatePackets()
        {
            // only update dirty items if we have any
            if (_dirtyBuffer.Any())
            {
                yield return new UpdateInterfaceItemPacket(this, Container, _dirtyBuffer);
                _dirtyBuffer.Clear();
            }
        }

        private void HandleItemChange(ItemChangeInfo info)
        {
            _dirtyBuffer.Add(info.Index);
        }

        private bool IsOurContainer(IItemContainer other)
            => ReferenceEquals(other, Container);

        public void ReceiveMessage(GameMessage msg)
        {
            switch (msg.Event)
            {
                case GameMessage.Type.ItemChange:
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
