using JetBrains.Annotations;

namespace CScape.Game.Interface
{
    /// <summary>
    /// Defines an interface that can be managed by an interface controller.
    /// </summary>
    public interface IManagedInterface : IInterfaceApi
    {
        /// <summary>
        /// Whether the interface is being shown at the given time or not.
        /// </summary>
        bool IsBeingShowed { get; }

        /// <summary>
        /// Returns details about the categorisation of the interface.
        /// </summary>
        InterfaceInfo Info { get; }

        /// <summary>
        /// Attempts to show this interface.
        /// </summary>
        /// <returns>True if interface was shown. False if the interface was already being shown as this was called.</returns>
        bool TryShow([NotNull] IInterfaceLifetimeManager manager);
    }
}