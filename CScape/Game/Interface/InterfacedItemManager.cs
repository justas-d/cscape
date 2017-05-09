using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CScape.Network;
using CScape.Network.Packet;
using JetBrains.Annotations;

namespace CScape.Game.Interface
{
    public class InterfacedItemManager : ItemManager, IContainerInterface
    {
        public int Id { get; }
        public bool IsRegistered => _api != null;
        public IItemManager Items => this;

        private IInterfaceManagerApiBackend _api;
        private ImmutableList<IPacket> _upds = ImmutableList<IPacket>.Empty;
        private ImmutableHashSet<int> _dirtyItems = ImmutableHashSet<int>.Empty;

        public InterfacedItemManager(int interfaceId, [NotNull] GameServer server, 
            [NotNull] IItemProvider provider) : base(server, provider)
        {
            Id = interfaceId;

            // initial updates
            PushUpdate(new ClearItemInterfacePacket(Id));
            PushUpdate(new MassSendInterfaceItemsPacket(this));
        }

        public override void ExecuteChangeInfo(ItemProviderChangeInfo info)
        {
            base.ExecuteChangeInfo(info);
            _dirtyItems.Add(info.Index);
        }

        public bool TryRegisterApi(IInterfaceManagerApiBackend api)
        {
            if (IsRegistered)
                return false;

            if (api.All.ContainsKey(Id))
                return false;

            api.All.Add(Id, this);
            return true;
        }

        public void UnregisterApi()
        {
            if (!IsRegistered)
                return;

            _api.All.Remove(Id);
            _api = null;
        }

        public IEnumerable<IPacket> GetUpdates()
        {
            var ret = _upds;

            if (_dirtyItems.Any())
            {
                ret = ret.Add(new UpdateInterfaceItemPacket(this, _dirtyItems));
                _dirtyItems = ImmutableHashSet<int>.Empty;
            }

            _upds = ImmutableList<IPacket>.Empty;
            return ret;
        }

        protected void PushUpdate(IPacket update)
            => _upds = _upds.Add(update);

        public bool Equals(IBaseInterface other)
            => Id == other.Id;

        public override int GetHashCode()
            => Id;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((IBaseInterface)obj);
        }
    }
}