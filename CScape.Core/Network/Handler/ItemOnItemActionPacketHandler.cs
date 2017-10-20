using CScape.Core.Data;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Entities.Message;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Interface;
using CScape.Core.Game.Interfaces;
using CScape.Core.Injection;

namespace CScape.Core.Network.Handler
{
    public sealed class ItemOnItemActionPacketHandler : IPacketHandler
    {
        public byte[] Handles { get; } = {53};

        public void Handle(Game.Entities.Entity entity, PacketMessage packet)
        {
            var interfaces = entity.Components.Get<InterfaceComponent>();
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
            IItemGameInterface GetContainer(int id)
            {
                if (!interfaces.All.TryGetValue(id, out var meta))
                {
                    entity.SystemMessage($"Unregistered Item on Item interface: {id}", SystemMessageFlags.Debug | SystemMessageFlags.Interface);
                    SendNIH();
                    return null;
                }

                var ret = meta.Interface as IItemGameInterface;
                return ret;
            }

            var containerA = GetContainer(interfaceIdA);
            var containerB = GetContainer(interfaceIdB);

            // verify we got all containers
            if (containerA == null || containerB == null) return;

            // validate indicies
            bool IsNotValidIdx(int idx, int max)
            {
                if (0 > idx || idx >= max)
                {
                    entity.SystemMessage($"Out of range item index on Item on Item: {idx}, max: {max}", SystemMessageFlags.Debug | SystemMessageFlags.Interface);
                    SendNIH();
                    return false;
                }

                return true;
            }

            if (IsNotValidIdx(idxA, containerA.Container.Provider.Count)) return;
            if (IsNotValidIdx(idxB, containerB.Container.Provider.Count)) return;

            // TODO : when do we handle item interaction?

            // data's valid, pass it on
            player.Interfaces.OnActionOccurred();
            defA.UseWith(player, containerA, idxA, containerB, idxB);
        }
    }
}