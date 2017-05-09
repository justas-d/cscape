using System.Collections.Generic;
using System.Collections.Immutable;
using CScape.Game.Entity;
using CScape.Network;

namespace CScape.Game.Interface
{
    public class ItemSidebarInterface : AbstractManagedInterface, IItemInterface, ISyncedInterface
    {
        private readonly ISyncedItemManager _items;
        private ImmutableList<IPacket> _updates = ImmutableList<IPacket>.Empty;

        public Player Player { get; }
        public IItemManager Items => _items;

        public ItemSidebarInterface(Player player, ISyncedItemManager items, 
            int interfaceId, int sidebarId) 
            : base(interfaceId, new InterfaceInfo(InterfaceInfo.InterfaceType.Sidebar, sidebarId))
        {
            Player = player;
            _items = items;

            PushUpdate();
        }

        public void PushUpdate(IPacket update) => _updates = _updates.Add(update);

        public IEnumerable<IPacket> GetUpdates()
        {
            var ret = _updates.AddRange(_items.GetUpdates());
            _updates = ImmutableList<IPacket>.Empty;
            return ret;
        }

        protected override bool InternalTryClose() => true;
    }
}
