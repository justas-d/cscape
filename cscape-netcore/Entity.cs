using System;

namespace cscape
{
    public abstract class Entity
    {
        /// <summary>
        /// Whether the entity is owned by an entity pool.
        /// </summary>
        public bool IsOwned { get; private set; }

        private int _instanceId;

        public int InstanceId
        {
            get { return _instanceId; }
            set
            {
                if(IsOwned)
                    throw new InvalidOperationException("Tried to set entity id when it's already owned.");

                _instanceId = value;
                IsOwned = true;
            }
        }

        public abstract PositionController Position { get; }
    }
}