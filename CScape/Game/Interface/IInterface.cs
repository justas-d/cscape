using System.Collections.Generic;
using CScape.Network;
using CScape.Network.Sync;
using JetBrains.Annotations;

namespace CScape.Game.Interface
{
    /// <summary>
    /// Defines an interface that can be synced, shown and closed.
    /// </summary>
    public interface IInterface
    {
        int InterfaceId { get; }
        [NotNull] IEnumerable<IPacket> GetUpdates { get; }

        /// <summary>
        /// Called after the interface has been closed.
        /// Should not be called from within the interface.
        /// todo : Call interface dispose
        /// </summary>
        void Dispose();
    }

    /// <summary>
    /// Defines an interface to which we can push updates.
    /// </summary>
    public interface IUpdatePushableInterface : IInterface
    {
        /// <summary>
        /// Pushes an update packet to this interface.
        /// </summary>
        void PushUpdate([NotNull] IPacket update);
    }
}