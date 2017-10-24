using System.Collections.Generic;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Game.Item;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Interface;

namespace CScape.Core.Network.Handler
{
    public sealed class ItemActionPacketHandlers : IPacketHandler
    {
        public Dictionary<int, ItemActionType> OpcodeToActionMap { get; } 
            = new Dictionary<int, ItemActionType>
        {
            {41, ItemActionType.Generic1},
            {122, ItemActionType.Generic2},
            {16, ItemActionType.Generic3},
            {87, ItemActionType.Drop},
            {145, ItemActionType.Remove } // TODO : figure out what ItemActionType.Remove is
        };

        public byte[] Handles { get; } = {122, 41, 16, 87, 145};

        public void Handle(IEntity entity, PacketMessage packet)
        {
            var interfaces = entity.GetInterfaces();
            if (interfaces == null)
            {
                entity.SystemMessage($"Attempted to handle an ItemAction packet but this entity does not have an InterfaceComponent", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Interface);
                return;
            }

            // read
            var interfId = packet.Data.ReadInt16();
            var idx = packet.Data.ReadInt16();
            var itemId = packet.Data.ReadInt16() + 1;

            entity.SystemMessage($"Action: interfId: {interfId} idx: {idx} id: {itemId}", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Item);

            // check if we have defined the action given by the current opcode
            if (!OpcodeToActionMap.ContainsKey(packet.Opcode))
            {
                entity.SystemMessage($"Undefined item action for action opcode: {packet.Opcode}", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Item);
                return;
            }

            // find interf
            if (!interfaces.All.TryGetValue(interfId, out var interfaceMetadata))
            {
                entity.SystemMessage($"ItemAction packet referenced interface which cannot be found: Id: {interfId}", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Interface);
                return;
            }

            // find container
            if (!(interfaceMetadata.Interface is IItemGameInterface itemInterface))
            {
                entity.SystemMessage($"ItemAction packet reference an interface which is not an item interface. Id: {interfId}", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Interface);
                return;
            }

            // verify idx
            var max = itemInterface.Container.Provider.Count;
            if (0 > idx || idx >= max)
            {
                entity.SystemMessage($"ItemAction packet gave an out of range index: {idx}. Max size: {max}", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Interface);
                return;
            }

            // verify itemId == item pointed at by idx
            var serverSideId = itemInterface.Container.Provider[idx];
            if (itemId != serverSideId.Id.ItemId)
            {
                entity.SystemMessage($"ItemAction server item id did not match the one in the given. Interface: {interfId} at given idx {idx} (client {itemId} != server {serverSideId})", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Item);
                return;
            }
            
            // determine action type by opcode
            var action = OpcodeToActionMap[packet.Opcode];

            entity.SendMessage(new ItemActionMessage(action, itemInterface.Container, interfaceMetadata, idx));

            // execute action
            def.OnAction(player, container, idx, action);
        }
    }
}
 