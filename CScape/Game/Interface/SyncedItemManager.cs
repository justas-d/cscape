using System;
using CScape.Network.Packet;
using JetBrains.Annotations;

namespace CScape.Game.Interface
{
    public class SyncedItemManager : ItemManager, IInterfacedItemManager
    {
        [NotNull] private readonly IManagedInterface _interf;

        public int ContainerInterfaceId { get; }

        public SyncedItemManager([NotNull] GameServer server, [NotNull] IItemProvider provider, int containerInterfaceId,
            [NotNull] IManagedInterface interf) : base(server, provider)
        {
            _interf = interf ?? throw new ArgumentNullException(nameof(interf));
            ContainerInterfaceId = containerInterfaceId;

            // push initial updates
            interf.PushUpdate(new ClearItemInterfacePacket(containerInterfaceId));
            interf.PushUpdate(new MassSendInterfaceItems(this));
        }

        public override void ExecuteChangeInfo(ItemProviderChangeInfo info)
        {
            base.ExecuteChangeInfo(info);
            // todo : sync item change info's
        }

    }
}