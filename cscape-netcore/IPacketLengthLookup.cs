namespace cscape
{
    public interface IPacketLengthLookup
    {
        void Reload();
        PacketLength GetIncoming(byte id);
        PacketLength GetOutgoing(byte id);
    }
}