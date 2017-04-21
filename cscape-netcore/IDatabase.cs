using CScape.Network.Packet;

namespace CScape
{
    public interface IDatabase
    {
        IPacketLengthLookup Packet { get; }
        IPlayerDatabase Player { get; }
    }
}