using System.Collections.Generic;
using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Game.Interface
{
    public interface IInterfaceController
    {
        [NotNull] Player Player { get; }

        /// <summary>
        /// The currently shown main interface.
        /// </summary>
        [CanBeNull]IInterface Main { get; }

        /// <summary>
        /// The currently shown sidebar interfaces.
        /// </summary>
        [NotNull] IReadOnlyDictionary<int, IInterface> Sidebar { get; }

        /// <summary>
        /// The currently shown input interface.
        /// </summary>
        [CanBeNull] IInterface Input { get;  }

        /// <summary>
        /// Attempts to show an interface. If we try to overwrite an interface, we return false. If we successfully show the interface, return true.
        /// </summary>
        bool TryShow([NotNull] IManagedInterface interf);
    }
}