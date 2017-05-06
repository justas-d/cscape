using CScape.Network;
using JetBrains.Annotations;

namespace CScape
{
    public interface IDatabase
    {
        [NotNull] IPacketLengthLookup Packet { get; }
        [NotNull] IPlayerDatabase Player { get; }
        [NotNull] IItemDatabase Item { get; }
    }
}