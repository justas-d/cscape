using System;
using CScape.Core.Data;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Interface;
using CScape.Core.Injection;

namespace CScape.Core.Network.Handler
{
    public sealed class ItemOnItemActionPacketHandler : IPacketHandler
    {
        public int[] Handles { get; } = {53};

        private IItemDefinitionDatabase _db;

        public ItemOnItemActionPacketHandler(IServiceProvider services)
        {
            _db = services.ThrowOrGet<IItemDefinitionDatabase>();
        }

        public void Handle(Player player, int opcode, Blob packet)
        {
            void SendNIH() => player.SendSystemChatMessage(Constant.NothingInterestingHappens);

            var idxB = packet.ReadInt16();
            var idxA = packet.ReadInt16();
            var itemIdB = packet.ReadInt16() + 1;
            var interfaceIdA = packet.ReadInt16();
            var itemIdA = packet.ReadInt16() + 1;
            var interfaceIdB = packet.ReadInt16();

            // try getting interfaces
            IContainerInterface GetContainer(int id)
            {
                var ret = player.Interfaces.TryGetById(id) as IContainerInterface;
                if (ret == null)
                {
                    player.Log.Warning(this, $"Unregistered Item on Item interface: {id}");
                    SendNIH();
                    return null;
                }
                return ret;
            }

            var containerA = GetContainer(interfaceIdA);
            var containerB = GetContainer(interfaceIdB);

            if (containerA == null || containerB == null) return;

            // validate indicies
            bool IsNotValidIdx(int idx, int max)
            {
                var ret = 0 > idx || idx >= max;
                if (ret)
                {
                    player.Log.Warning(this, $"Out of range item index on Item on Item: {idx}, max: {max}");
                    SendNIH();
                }

                return ret;
            }

            if (IsNotValidIdx(idxA, containerA.Items.Size)) return;
            if (IsNotValidIdx(idxB, containerB.Items.Size)) return;

            // compare and validate ids
            bool IsNotValidId(int clientId, IItemProvider provider, int idx)
            {
                var serverId = provider.GetId(idx);
                var ret = clientId != serverId;
                if (ret)
                {
                    player.Log.Warning(this, $"item on item id mismatch: server: {serverId} client: {clientId}");
                    SendNIH();
                }
                return ret;
            }

            if (IsNotValidId(itemIdA, containerA.Items.Provider, idxA)) return;
            if (IsNotValidId(itemIdB, containerB.Items.Provider, idxB)) return;

            // get def of A
            var defA = _db.Get(itemIdA);

            if (defA == null)
            {
                player.Log.Warning(this, $"Undefined itemA on item on item, id: {itemIdA}");
                SendNIH();
                return;
            }

            // data's valid, pass it on
            player.Interfaces.OnActionOccurred();
            defA.UseWith(player, containerA, idxA, containerB, idxB);
        }
    }
}