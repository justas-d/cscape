using System.Collections.Generic;
using CScape.Network;
using JetBrains.Annotations;

namespace CScape.Game.Interface
{
    /// <summary>
    /// Defines an interface that can be synced to a player.
    /// </summary>
    public interface ISyncedInterface : IInterface
    {
        /// <summary>
        /// Returns the accumulated updates for this interface.
        /// </summary>
        /// <returns></returns>
        [NotNull] IEnumerable<IPacket> GetUpdates();

        /// <summary>
        /// Pushes an packet update to the interface update queue.
        /// </summary>
        /// <param name="update"></param>
        void PushUpdate([NotNull] IPacket update);
    }

    public interface ISyncedItemManager : IItemManager, ISyncedInterface { }
}