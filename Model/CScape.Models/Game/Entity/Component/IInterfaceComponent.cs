using System.Collections.Generic;
using CScape.Models.Game.Interface;
using JetBrains.Annotations;

namespace CScape.Models.Game.Entity.Component
{
    /// <summary>
    /// Defines a component which manages and catalogues the entity's visible interfaces.
    /// </summary>
    public interface IInterfaceComponent : IEntityComponent
    {
        /// <summary>
        /// All of this entity's open interfaces.
        /// </summary>
        [NotNull]
        IReadOnlyDictionary<int, InterfaceMetadata> All { get; }

        /// <summary>
        /// The currently open chat interface.
        /// </summary>
        [CanBeNull]
        IGameInterface Chat { get; }

        /// <summary>
        /// The currently open input interface.
        /// </summary>
        [CanBeNull]
        IGameInterface Input { get; }

        /// <summary>
        /// The currently open main interface.
        /// </summary>
        [CanBeNull]
        IGameInterface Main { get; }

        /// <summary>
        /// The currently open sidebar interfaces. Entries can be null.
        /// </summary>
        [NotNull]
        IList<IGameInterface> Sidebar { get; }

        /// <summary>
        /// Resolves whether the given interface defined by <see cref="meta"/> can be shown to this entity.
        /// </summary>
        /// <returns>True if can be shown, false otherwise.</returns>
        bool CanShow(InterfaceMetadata meta);

        /// <summary>
        /// Attempts to close an interface mapped to the given interface id <see cref="id"/>, if it is open.
        /// </summary>
        ///<returns>True if succesfully, removed false otherwise.</returns>
        bool Close(int id);

        /// <summary>
        /// Shows the given interface to the entity. <see cref="CanShow"/> must be satisfied for <see cref="meta"/>.
        /// </summary>
        ///<returns>True if succesfully shown, false otherwise.</returns>
        bool Show(InterfaceMetadata meta);
    }
}