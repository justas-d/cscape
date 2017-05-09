using System.Collections.Generic;
using CScape.Game.Entity;
using CScape.Network;
using JetBrains.Annotations;

namespace CScape.Game.Interface
{
    public interface IInterfaceController
    {
        [NotNull] Player Player { get; }

        /// <summary>
        /// The currently shown main interface.
        /// </summary>
        [CanBeNull]IInterfaceApi Main { get; }

        /// <summary>
        /// The currently shown sidebar interfaces.
        /// </summary>
        [NotNull] IReadOnlyDictionary<int, IInterfaceApi> Sidebar { get; }

        /// <summary>
        /// The currently shown input interface.
        /// </summary>
        [CanBeNull] IInterfaceApi Input { get;  }

        /// <summary>
        /// Attempts to show an interface. If we try to overwrite an interface, we return false. If we successfully show the interface, return true.
        /// </summary>
        bool TryShow([NotNull] IManagedInterface interf);

        /// <summary>
        /// Returns the updates for all managed interfaces.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IPacket> GetUpdates();

        void HandleButton(int interfaceId, int buttonId);

        // todo : handle packet id 40 client -> server sent when player advances dialog
    }
}