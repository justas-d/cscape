using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CScape.Network;
using CScape.Network.Packet;
using JetBrains.Annotations;

namespace CScape.Game.Interface
{
    /// <summary>
    /// Defines an item manager that can be synced, as an interface item container, to a player.
    /// </summary>
    public class SyncedItemManager : ItemManager, ISyncedItemManager
    {
        public int InterfaceId { get; }

        private ImmutableList<IPacket> _updates = ImmutableList<IPacket>.Empty;
        private ImmutableHashSet<int> _dirtyItems = ImmutableHashSet<int>.Empty;

        public SyncedItemManager([NotNull] GameServer server, int containerInterfaceId, 
            [NotNull] IItemProvider provider) : base(server, provider)
        {
            InterfaceId = containerInterfaceId;

            // initial updates
            PushUpdate(new ClearItemInterfacePacket(containerInterfaceId));
            PushUpdate(new MassSendInterfaceItemsPacket(this));
        }

        public override void ExecuteChangeInfo(ItemProviderChangeInfo info)
        {
            base.ExecuteChangeInfo(info);
            _dirtyItems.Add(info.Index);
        }

        public bool Equals(IInterface other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return InterfaceId == other.InterfaceId;
        }

        public IEnumerable<IPacket> GetUpdates()
        {
            var ret = _updates;

            if (_dirtyItems.Any())
            {
                ret = ret.Add(new UpdateInterfaceItemPacket(this, _dirtyItems));
                _dirtyItems = ImmutableHashSet<int>.Empty;
            }

            _updates = ImmutableList<IPacket>.Empty;
            return ret;
        }

        public void PushUpdate(IPacket update) => _updates = _updates.Add(update);
    }
}