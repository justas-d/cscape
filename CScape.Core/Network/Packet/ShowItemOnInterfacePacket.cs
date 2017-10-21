using CScape.Models.Data;

namespace CScape.Core.Network.Packet
{
    public sealed class ShowItemOnInterfacePacket : IPacket
    {
        private readonly int _id;
        private readonly int _zoom;
        private readonly int _itemId;

        public const int Id = 246;

        public ShowItemOnInterfacePacket(int id, int zoom, int itemId)
        {
            _id = id;
            _zoom = zoom;
            _itemId = itemId;
        }

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(Id);   

            stream.Write16((short)_id);
            stream.Write16((short)_zoom);
            stream.Write16((short)_itemId);

            stream.EndPacket();
        }
    }
}