using System.Collections.Generic;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Game.Entity;
using CScape.Core.Network;
using JetBrains.Annotations;

namespace CScape.Core.Game.Interface
{
    /// <summary>
    /// Defines an interface manager which syncs and manages the lifetime of interfaces.
    /// </summary>
    public interface IInterfaceManager
    {
        [CanBeNull] IShowableInterface Main { get; }
        [NotNull] IReadOnlyList<IShowableInterface> Sidebar { get; }
        [CanBeNull] IShowableInterface Chat { get; }
        [CanBeNull] IShowableInterface Input { get; }

        [NotNull] IReadOnlyDictionary<int, IBaseInterface> All { get; }

        [CanBeNull] IBaseInterface TryGetById(int id);

        /// <summary>
        /// Attempts to show a showable api interface.
        /// </summary>
        /// <returns>True if registered successfully, false otherwise.</returns>
        bool TryShow<T>([NotNull] T interf) where T : IApiInterface, IShowableInterface;

        /// <summary>
        /// Attempts to register an api interface.
        /// </summary>
        /// <param name="interf"></param>
        /// <returns>True if registered successfully, false otherwise.</returns>
        bool TryRegister([NotNull]IApiInterface interf);

        /// <summary>
        /// Attempts to unregister an api interface by its id.
        /// </summary>
        bool TryUnregister(int id);

        /// <summary>
        /// Taking into account the current state of the manager, 
        /// determines and returns whether the manager can show the given 
        /// type of interface and, optionall if the interface is a sidebar, 
        /// whether it can be shown at the given sidebar index.
        /// </summary>
        bool CanShow(InterfaceType type, int? sidebarSlotIndex = 0);

        void HandleButton(Player player, int interfaceId, int buttonId);
        void OnActionOccurred();

        IEnumerable<IPacket> GetUpdates();
    }
}