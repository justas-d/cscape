using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CScape.Network;
using JetBrains.Annotations;

namespace CScape.Game.Interface
{
    public abstract class SingleUserApiInterface : IApiInterface
    {
        public int Id { get; }
        public bool IsRegistered => Api != null;

        [CanBeNull] protected IInterfaceManagerApiBackend Api { get; private set; }
        private ImmutableList<IPacket> _upds = ImmutableList<IPacket>.Empty;

        protected SingleUserApiInterface(int id)
        {
            Id = id;
        }

        public bool TryRegisterApi(IInterfaceManagerApiBackend api)
        {
            if (IsRegistered)
                return false;

            if (!InternalRegister(api))
                return false;

            Api = api;

            return true;
        }

        protected abstract bool InternalRegister(IInterfaceManagerApiBackend api);
        protected abstract void InternalUnregister();

        public void UnregisterApi()
        {
            if(!IsRegistered)
                return;

            InternalUnregister();

            Api = null;
        }

        public IEnumerable<IPacket> GetUpdates()
        {
            var ret = _upds;
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
            if (Object.ReferenceEquals(null, obj)) return false;
            if (Object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IBaseInterface)obj);
        }

    }
}