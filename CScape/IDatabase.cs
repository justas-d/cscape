using System;
using CScape.Network;
using JetBrains.Annotations;

namespace CScape
{
    public interface IDatabase : IDisposable
    {
        [NotNull] IPacketLengthLookup Packet { get; }
        [NotNull] IPlayerDatabase Player { get; }
        [NotNull] IItemDatabase Item { get; }
    }
}