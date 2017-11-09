namespace CScape.Core.Network
{
    public interface IPacketDatabase
    {
        PacketLength GetIncoming(byte id);
        PacketLength GetOutgoing(byte id);
    }
}