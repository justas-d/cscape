using System;

namespace CScape.Game.Interface
{
    public abstract class AbstractManagedInterface : IManagedInterface
    {
        public bool IsBeingShowed => _manager != null;
        public InterfaceInfo Info { get; }
        public int InterfaceId { get; }

        private IInterfaceLifetimeManager _manager;

        protected AbstractManagedInterface(int interfaceId, InterfaceInfo info)
        {
            InterfaceId = interfaceId;
            Info = info;
        }

        bool IManagedInterface.TryShow(IInterfaceLifetimeManager manager)
        {
            if(IsBeingShowed)
                return false;

            _manager = manager;
            return true;
        }
      
        public bool Equals(IInterface other)
        {
            if (other == null) return false;
            if (Object.ReferenceEquals(this, other)) return true;
            return this.InterfaceId == other.InterfaceId;
        }

        public bool TryClose()
        {
            if (InternalTryClose())
            {
                _manager.Close(this);
                _manager = null;
                return true;
            }

            return false;
        }

        protected abstract bool InternalTryClose();
    }
}