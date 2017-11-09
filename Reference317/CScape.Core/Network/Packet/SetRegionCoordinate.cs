using CScape.Models.Data;

namespace CScape.Core.Network.Packet
{
    public sealed class SetRegionCoordinate : IPacket
    {
        private readonly short _x;
        private readonly short _y;
        public const int Id = 73;

        public SetRegionCoordinate(short x, short y)
        {
            _x = x;
            _y = y;
        }

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(Id);
            stream.Write16(_x);
            stream.Write16(_y);
            stream.EndPacket();
        }
    }
}
