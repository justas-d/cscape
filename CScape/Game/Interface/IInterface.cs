using System.Collections.Generic;
using CScape.Network.Sync;

namespace CScape.Game.Interface
{
    /// <summary>
    /// Defines an interface that can be synced, shown and closed.
    /// </summary>
    public interface IInterface
    {
        int InterfaceId { get; }
        IEnumerable<IPacket> GetUpdates { get; }

        /// <summary>
        /// Called after the interface has been closed.
        /// Should not be called from within the interface.
        /// todo : Call interface dispose
        /// </summary>
        void Dispose();
    }
}