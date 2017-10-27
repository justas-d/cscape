﻿using CScape.Models.Game.Entity;

namespace CScape.Core.Game.Entity
{
    public abstract class InstanceFactory
    {
        public int InstanceNum { get; }

        // instance id lookup
        protected IEntityHandle[] InstanceLookup { get; }

        public const int InvalidId = -1;

        public InstanceFactory(int instanceNum)
        {
            InstanceNum = instanceNum;
            InstanceLookup = new IEntityHandle[instanceNum];
        }

        /// <summary>
        /// Finds and returns the next free player id.
        /// </summary>
        /// <returns><see cref="InvalidId"/> if failed to get id, otherwise the actual id.</returns>
        protected int GetId()
        {
            for (int i = 0; i < InstanceNum; i++)
            {
                if (InstanceLookup[i] == null)
                    return i;
            }

            return InvalidId;
        }
    }
}