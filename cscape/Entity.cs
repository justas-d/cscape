using System;

namespace cscape
{
    public abstract class Entity
    {
        /// <summary>
        /// Whether the entity is owned by an entity pool.
        /// </summary>
        public bool IsOwned { get; private set; }

        private int _uniqueId;

        public int UniqueId
        {
            get { return _uniqueId; }
            set
            {
                if(IsOwned)
                    throw new InvalidOperationException("Tried to set entity id when it's already owned.");

                _uniqueId = value;
                IsOwned = true;
            }
        }
    }
}