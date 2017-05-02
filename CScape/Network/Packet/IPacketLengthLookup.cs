namespace CScape.Network.Packet
{
    public interface IPacketLengthLookup
    {
        void Reload();
        PacketLength GetIncoming(byte id);
        PacketLength GetOutgoing(byte id);
    }
}