using CScape.Core.Data;

namespace CScape.Core.Network.Packet
{
    public sealed class LogoffPacket : IPacket
    {
        public const int Id = 109;

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(Id);
            stream.EndPacket();
        }

        public static LogoffPacket Static => new LogoffPacket();
    }
}