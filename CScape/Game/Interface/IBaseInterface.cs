using System;
using System.Collections.Generic;
using CScape.Network;
using JetBrains.Annotations;

namespace CScape.Game.Interface
{
    /// <summary>
    /// Defines a base interface that all interfaces stem from.
    /// </summary>
    public interface IBaseInterface : IEquatable<IBaseInterface>
    {
        /// <summary>
        /// The unique id of the interface.
        /// </summary>
        int Id { get; }

        [NotNull] IEnumerable<IPacket> GetUpdates();
    }
}
