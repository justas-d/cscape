using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CScape.Core.Network;
using CScape.Core.Network.Packet;
using JetBrains.Annotations;

namespace CScape.Core.Game.Interface
{
    public abstract class AbstractSyncedItemManager : IItemContainer, IContainerInterface
    {
        public IItemProvider Provider { get; }
        public IItemContainer Items => this;

        public int Size { get; }
        public int Id { get; }
        public bool IsRegistered => _api != null;

        private IInterfaceManagerApiBackend _api;
        private ImmutableList<IPacket> _upds = ImmutableList<IPacket>.Empty;
        private ImmutableHashSet<int> _dirtyItems = ImmutableHashSet<int>.Empty;

        protected AbstractSyncedItemManager(int interfaceId, [NotNull] IItemProvider provider)
        {
            Id = interfaceId;
            Provider = provider;
            Size = provider.Count;

            // initial updates
            PushUpdate(new ClearItemInterfacePacket(Id));
            PushUpdate(new MassSendInterfaceItemsPacket(this));
        }

        public bool ExecuteChangeInfo(ItemProviderChangeInfo info)
        {
            if (!info.IsValid)
                return false;

            _dirtyItems = _dirtyItems.Add(info.Index);
            return InternalExecuteChangeInfo(info);
        }

        protected abstract bool InternalExecuteChangeInfo(ItemProviderChangeInfo info);
        public abstract int Contains(int id);
        public abstract ItemProviderChangeInfo CalcChangeInfo(int id, int deltaAmount);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((IBaseInterface) obj);
        }

        protected void PushUpdate(IPacket update)
            => _upds = _upds.Add(update);

        public bool Equals(IBaseInterface other)
            => Id == other.Id;

        public override int GetHashCode()
            => Id;

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
    }
}