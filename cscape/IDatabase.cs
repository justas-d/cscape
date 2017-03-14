namespace cscape
{
    public interface IDatabase
    {
        IPacketLengthLookup Packet { get; }
        IPlayerDatabase Player { get; }
    }
}