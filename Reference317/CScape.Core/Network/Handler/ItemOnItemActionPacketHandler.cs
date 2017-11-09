using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Message;
using CScape.Models;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Interface;
using System.Diagnostics;

namespace CScape.Core.Network.Handler
{
    public sealed class ItemOnItemActionPacketHandler : IPacketHandler
    {
        public byte[] Handles { get; } = {53};

        public void Handle(IEntity entity, PacketMessage packet)
        {
            var interfaces = entity.GetInterfaces();
            if (interfaces == null)
                return;

            void SendNIH() => entity.SystemMessage(Constant.NothingInterestingHappens);

            var idxB = packet.Data.ReadInt16();
            var idxA = packet.Data.ReadInt16();
            var itemIdB = packet.Data.ReadInt16() + 1;
            var interfaceIdA = packet.Data.ReadInt16();
            var itemIdA = packet.Data.ReadInt16() + 1;
            var interfaceIdB = packet.Data.ReadInt16();

            // make sure we're not operating on the same item instance
            if (idxA == idxB) return;

            // try getting interfaces
            (InterfaceMetadata?, IItemGameInterface) GetContainer(int id)
            {
                if (!interfaces.All.TryGetValue(id, out var meta))
                {
                    entity.SystemMessage($"Unregistered Item on Item interface: {id}", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Interface);
                    SendNIH();
                    return (null, null);
                }

                var ret = meta.Interface as IItemGameInterface;
                return (meta, ret);
            }

            var (metaA, containerA) = GetContainer(interfaceIdA);
            var (metaB, containerB) = GetContainer(interfaceIdB);

            // verify we got all containers
            if (containerA == null || containerB == null) return;

            Debug.Assert(metaA != null && metaB != null);

            // validate indicies
            bool IsNotValidIdx(int idx, int max)
            {
                if (0 > idx || idx >= max)
                {
                    entity.SystemMessage($"Out of range item index on Item on Item: {idx}, max: {max}", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Interface);
                    SendNIH();
                    return false;
                }

                return true;
            }

            if (IsNotValidIdx(idxA, containerA.Container.Provider.Count)) return;
            if (IsNotValidIdx(idxB, containerB.Container.Provider.Count)) return;

            entity.SendMessage(new ItemOnItemMessage(
                metaA.Value, containerA.Container, idxA, 
                metaB.Value, containerB.Container, idxB));
        }
    }
}