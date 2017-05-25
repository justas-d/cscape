using CScape.Core.Data;
using CScape.Core.Game.Entity;

namespace CScape.Core.Network.Packet
{
    /// <summary>
    /// Encodes a packet which removes a ground item whose id matches the given one at the given packed x/y region coords
    /// </summary>
    public class DeleteGroundItemPacket : BaseGroundObjectPacket
    {
        private readonly int _id;

        public DeleteGroundItemPacket(
            GroundItem item,
            (int x, int y) off)
            :this(item.ItemId, off.x,off.y)
        {
            
        }

        public DeleteGroundItemPacket(
            int id,
            int offX, int offY)
            :base(offX, offY)
        {
            _id = id - 1;
        }

        public const int Id = 156;

        protected override void InternalSend(OutBlob stream)
        {
            stream.BeginPacket(Id);

            stream.Write(PackedPos);
            stream.Write16((short)_id);

            stream.EndPacket();
        }


    }
}