using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CScape.Network;
using JetBrains.Annotations;

namespace CScape.Game.Interface
{
    public struct InterfaceInfo
    {
        public InterfaceInfo(InterfaceType type, int sidebarIndex)
        {
            Type = type;
            SidebarIndex = sidebarIndex;
        }

        public enum InterfaceType
        {
            Main,
            Input,
            Sidebar
        }

        public InterfaceType Type { get; }
        public int SidebarIndex { get; }
    }

    /// <summary>
    /// Defines an interface that can be uniquely identified.
    /// </summary>
    public interface IInterface : IEquatable<IInterface>
    {
        /// <summary>
        /// The id of the interface being shown as stored in the client cache.
        /// </summary>
        int InterfaceId { get; }

        /// <summary>
        /// Attepts to close the interface.
        /// </summary>
        /// <returns>True on success, false otherwise.</returns>
        bool TryClose();
    }

    /// <summary>
    /// Defines an interface that can be managed by an interface controller.
    /// </summary>
    public interface IManagedInterface : IInterface
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
        /// Returns the accumulated updates for this interface.
        /// </summary>
        /// <returns></returns>
        [NotNull] IEnumerable<IPacket> GetUpdates();

        /// <summary>
        /// Attempts to show this interface.
        /// </summary>
        /// <returns>True if interface was shown. False if the interface was already being shown as this was called.</returns>
        bool TryShow([NotNull] IInterfaceLifetimeManager manager);

        /// <summary>
        /// Pushes an packet update to the interface update queue.
        /// </summary>
        /// <param name="update"></param>
        void PushUpdate([NotNull] IPacket update);
    }

    public abstract class AbstractManagedInterface : IManagedInterface
    {
        public bool IsBeingShowed => _manager != null;

        public int InterfaceId { get; }
        public InterfaceInfo Info { get; }

        private ImmutableList<IPacket> _updates = ImmutableList<IPacket>.Empty;
        private IInterfaceLifetimeManager _manager;

        protected AbstractManagedInterface(int interfaceId, InterfaceInfo info)
        {
            InterfaceId = interfaceId;
            Info = info;
        }

        public IEnumerable<IPacket> GetUpdates()
        {
            var ret = _updates;
            _updates = ImmutableList<IPacket>.Empty;
            return ret;
        }

        bool IManagedInterface.TryShow(IInterfaceLifetimeManager manager)
        {
            if(IsBeingShowed)
                return false;

            _manager = manager;
            return true;
        }

        public bool TryClose()
        {
            if (InternalTryClose())
            {
                _manager.Close(this);
                _manager = null;
                return true;
            }

            return false;
        }

        public void PushUpdate(IPacket update) => _updates = _updates.Add(update);
        protected abstract bool InternalTryClose();

        public bool Equals(IInterface other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.InterfaceId == this.InterfaceId;
        }
    }
}