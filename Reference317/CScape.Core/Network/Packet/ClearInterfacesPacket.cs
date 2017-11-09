using CScape.Models.Data;

namespace CScape.Core.Network.Packet
{
    public class ClearInterfacesPacket : IPacket
    {
        public static ClearInterfacesPacket Singleton { get; } = new ClearInterfacesPacket();

        public const int Id = 219;

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(219);
            stream.EndPacket();
        }
    }
}