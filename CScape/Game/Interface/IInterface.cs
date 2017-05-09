using System;

namespace CScape.Game.Interface
{
    /// <summary>
    /// Defines an interface that can be uniquely identified.
    /// </summary>
    public interface IInterface : IEquatable<IInterface>
    {
        /// <summary>
        /// The id of the interface being shown as stored in the client cache.
        /// </summary>
        int InterfaceId { get; }
    }

    public interface IInterfaceApi : IInterface
    {
        /// <summary>
        /// Attepts to close the interface.
        /// </summary>
        /// <returns>True on success, false otherwise.</returns>
        bool TryClose();
    }
}