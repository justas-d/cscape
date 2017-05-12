using System;
using System.Collections.Generic;
using CScape.Core.Network;
using JetBrains.Annotations;

namespace CScape.Core.Game.Interface
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
