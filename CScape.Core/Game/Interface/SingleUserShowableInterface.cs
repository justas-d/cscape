using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CScape.Core.Network;
using JetBrains.Annotations;

namespace CScape.Core.Game.Interface
{
    public abstract class SingleUserShowableInterface : IApiInterface, IShowableInterface
    {
        public int Id { get; }
        public bool IsRegistered => Api != null;

        [CanBeNull] protected IInterfaceManagerApiBackend Api { get; private set; }
        private ImmutableList<IPacket> _upds = ImmutableList<IPacket>.Empty;

        protected SingleUserShowableInterface(int id, [CanBeNull] IButtonHandler buttonHandler = null)
        {
            Id = id;
            ButtonHandler = buttonHandler;
        }

        public bool TryRegisterApi(IInterfaceManagerApiBackend api)
        {
            if (IsRegistered)
                return false;

            if (!InternalRegister(api))
                return false;

            Api = api;
            Api.NotifyOfRegister(this);

            return true;
        }

        protected abstract bool InternalRegister(IInterfaceManagerApiBackend api);
        protected abstract void InternalUnregister();

        /// <summary>
        /// Handles figuring out whether we can close.
        /// </summary>
        protected abstract bool CanCloseRightNow();

        /// <summary>
        /// Handles the interface specific close logic.
        /// </summary>
        /// <returns></returns>
        protected abstract void InternalClose();

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

        public bool TryClose()
        {
            if (Api == null)
                return false;

            if (!CanCloseRightNow())
                return false;

            InternalClose();
            Api.NotifyOfClose(this);
            return true;
        }

        public abstract  void Show();

        public IButtonHandler ButtonHandler { get; }
    }
}