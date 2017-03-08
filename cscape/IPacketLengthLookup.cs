namespace cscape
{
    public interface IPacketLengthLookup
    {
        PacketLength GetIncoming(byte id);
        PacketLength GetOutgoing(byte id);
    }
}