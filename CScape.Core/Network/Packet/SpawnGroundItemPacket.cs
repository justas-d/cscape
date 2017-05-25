using CScape.Core.Data;
using CScape.Core.Game.Entity;

namespace CScape.Core.Network.Packet
{
    /// <summary>
    /// Encodes an a packet which spawns a ground item.
    /// </summary>
    public class SpawnGroundItemPacket : AbstractBaseGroundItemPacket
    {
        public const int Id = 44;

        public SpawnGroundItemPacket(
            GroundItem item,
            (int x, int y) off)
            : this(item.ItemId, item.ItemAmount,
                  off.x, off.y)
        {
            
        }

        public SpawnGroundItemPacket(
            (int id, int amount) item,
            int offX, int offY)
            : base(item, offX, offY)
        {

        }

        public SpawnGroundItemPacket(
            int id, int amount,
            int offX, int offY)
            : base(id, amount, offX, offY)
        {

        }

        protected override void InternalSend(OutBlob stream)
        {
            stream.BeginPacket(Id);

            stream.Write16(ItemId);
            stream.Write16(Amount);
            stream.Write(PackedPos);

            stream.EndPacket();
        }
    }
}
 